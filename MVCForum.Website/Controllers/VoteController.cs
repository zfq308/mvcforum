﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Controllers
{

    public class VoteController : BaseController
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 建构式

        private readonly IPostService _postService;
        private readonly IVoteService _voteService;
        private readonly ITopicService _topicService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IBadgeService _badgeService;

        public VoteController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            IPostService postService,
            IVoteService voteService,
            ISettingsService settingsService,
            ITopicService topicService,
            IMembershipUserPointsService membershipUserPointsService,
            IBadgeService badgeService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _voteService = voteService;
            _topicService = topicService;
            _membershipUserPointsService = membershipUserPointsService;
            _badgeService = badgeService;
        }
        #endregion


        public ActionResult Index()
        {
            return View();
        }

        #region 点赞和批评

        /// <summary>
        /// 为这个帖子点赞
        /// </summary>
        /// <param name="voteUpViewModel"></param>
        [HttpPost]
        [Authorize]
        public void VoteUpPost(VoteUpViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                // 检查当前用户的状态，确保不是被锁定状态和等待审核状态
                if (LoggedOnReadOnlyUser.IsLockedOut | !LoggedOnReadOnlyUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // 当前待评价的帖子
                    var post = _postService.Get(voteUpViewModel.Post);
                    var voter = MembershipService.GetUser(LoggedOnReadOnlyUser.Id); // 当前用户

                    // Also get the user that wrote the post
                    var postWriter = post.User;

                    // Mark the post up or down
                    MarkPostUpOrDown(post, postWriter, voter, PostType.Positive);

                    try
                    {
                        unitOfWork.Commit();
                    }

                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
        }

        /// <summary>
        /// 踩这个帖子
        /// </summary>
        /// <param name="voteDownViewModel"></param>
        [HttpPost]
        [Authorize]
        public void VoteDownPost(VoteDownViewModel voteDownViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                // 检查当前用户的状态，确保不是被锁定状态和等待审核状态
                if (LoggedOnReadOnlyUser.IsLockedOut | !LoggedOnReadOnlyUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // 当前待评价的帖子
                    var post = _postService.Get(voteDownViewModel.Post);
                    var voter = MembershipService.GetUser(LoggedOnReadOnlyUser.Id); // 当前用户

                    // Also get the user that wrote the post
                    var postWriter = post.User;

                    // Mark the post up or down
                    MarkPostUpOrDown(post, postWriter, voter, PostType.Negative);

                    try
                    {
                        unitOfWork.Commit();
                    }

                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
        }

        private void MarkPostUpOrDown(Post post, MembershipUser postWriter, MembershipUser voter, PostType postType)
        {
            var settings = SettingsService.GetSettings();
            // Check this user is not the post owner
            if (voter.Id != postWriter.Id)
            {
                // Not the same person, now check they haven't voted on this post before
                var votes = post.Votes.Where(x => x.VotedByMembershipUser.Id == LoggedOnReadOnlyUser.Id).ToList();
                if (votes.Any())
                {
                    // Already voted, so delete the vote and remove the points
                    var votesToDelete = new List<Vote>();
                    votesToDelete.AddRange(votes);
                    foreach (var vote in votesToDelete)
                    {
                        _voteService.Delete(vote);
                    }

                    // Update the post with the new points amount
                    var newPointTotal = (postType == PostType.Negative) ? (post.VoteCount + 1) : (post.VoteCount - 1);
                    post.VoteCount = newPointTotal;
                }
                else
                {
                    // Points to add or subtract to a user
                    var usersPoints = (postType == PostType.Negative) ? (-settings.PointsDeductedNagativeVote) : (settings.PointsAddedPostiveVote);

                    // Update the post with the new vote of the voter
                    var vote = new Vote
                    {
                        Post = post,
                        User = postWriter,
                        Amount = (postType == PostType.Negative) ? (-1) : (1),
                        VotedByMembershipUser = voter,
                        DateVoted = DateTime.Now
                    };
                    _voteService.Add(vote);

                    // Update the users points who wrote the post
                    _membershipUserPointsService.Add(new MembershipUserPoints
                    {
                        Points = usersPoints,
                        User = postWriter,
                        PointsFor = PointsFor.Vote,
                        PointsForId = vote.Id
                    });

                    // Update the post with the new points amount
                    var newPointTotal = (postType == PostType.Negative) ? (post.VoteCount - 1) : (post.VoteCount + 1);
                    post.VoteCount = newPointTotal;
                }
            }
        }

        private enum PostType
        {
            Positive,
            Negative,
        };

        #endregion

        [HttpPost]
        [Authorize]
        public void MarkAsSolution(MarkAsSolutionViewModel markAsSolutionViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                // Quick check to see if user is locked out, when logged in
                if (LoggedOnReadOnlyUser.IsLockedOut | !LoggedOnReadOnlyUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {

                    // Get a db user
                    var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);

                    // Firstly get the post
                    var post = _postService.Get(markAsSolutionViewModel.Post);

                    // Person who created the solution post
                    var solutionWriter = post.User;

                    // Get the post topic
                    var topic = post.Topic;

                    // Now get the current user
                    var marker = loggedOnUser;
                    try
                    {
                        var solved = _topicService.SolveTopic(topic, post, marker, solutionWriter);

                        if (solved)
                        {
                            unitOfWork.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }

                }
            }
        }

        /// <summary>
        /// 取得全部点赞者清单
        /// </summary>
        /// <param name="voteUpViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult GetVoters(VoteUpViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                var post = _postService.Get(voteUpViewModel.Post);
                var positiveVotes = post.Votes.Where(x => x.Amount > 0);
                var viewModel = new ShowVotersViewModel { Votes = positiveVotes.ToList() };
                return PartialView(viewModel);
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult GetVotes(VoteUpViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                var post = _postService.Get(voteUpViewModel.Post);
                var positiveVotes = post.Votes.Count(x => x.Amount > 0);
                var negativeVotes = post.Votes.Count(x => x.Amount <= 0);
                var viewModel = new ShowVotesViewModel { DownVotes = negativeVotes, UpVotes = positiveVotes };
                return PartialView(viewModel);
            }
            return null;
        }
    }
}
