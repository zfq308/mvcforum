一些常见的MVC功能
1. 输出一个Partial区块到前台页面

2. 调到用户账户编辑页
 <a href="@Url.Action("Edit", "Members", new { id = Model.CurrentUser.Id })">@Html.LanguageString("Buttons.Edit") @Model.CurrentUser.UserName</a>




TODO List

1. 