/*global $, jQuery, alert*/
(function ($) {
    'use strict';
    $.fn.stickUp = function (option) {
        var self, originaltop, originalleft, outw, oldp, oldf, outh, createId;
        originaltop = $(this).offset().top;
        originalleft = $(this).offset().left;
        outw = $(this).outerWidth();
        oldp = $(this).css("position");
        oldf = $(this).css("float");
        outh = $(this).outerHeight();

        //createId = "stick" + (1 + Math.floor(Math.random() * 9999999999));
        var replaceDiv = $("<div/>", {
            css: {
                width: outw,
                height: outh,
                position: oldp,
                float: oldf
            }
        });

        self = this;
        $(window).scroll(function () {
            if ($(self).css("position") !== "fixed") {
                if ($(self).offset().top <= $(window).scrollTop()) {
                    $(self).outerWidth(outw);
                    $(self).css({
                        position: "fixed",
                        top: 0,
                        left: originalleft
                    });
                    $(self).after(replaceDiv);
                }
            } else {
                if (originaltop > $(window).scrollTop()) {
                    $(self).css({
                        position: oldp
                    });

                    $(replaceDiv).remove();
                }
            }
        });
    };
}(jQuery));