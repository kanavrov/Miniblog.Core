(function (window, document) {

    // Lazy load stylesheets
    requestAnimationFrame(function () {
        var stylesheets = document.querySelectorAll("link[as=style]");

        for (var i = 0; i < stylesheets.length; i++) {
            var link = stylesheets[i];
            link.setAttribute("rel", "stylesheet");
            link.removeAttribute("as");
        }
    });

    // Show comment form. It's invisible by default in case visitor
    // has disabled javascript
    var commentForm = document.querySelector("#comments form");
    if (commentForm) {
        commentForm.classList.add("js-enabled");

        commentForm.addEventListener("submit", function (e) {
            this.querySelector("input[type=submit]").value = window.i18n("Comments.Posting");
            var elements = this.elements;
            for (var i = 0; i < elements.length; ++i) {
                elements[i].readOnly = true;
            }
        });
    }

    // Expand comment form
    var content = document.querySelector("#comments textarea");
    if (content) {
        content.addEventListener("focus", function () {
            document.querySelector(".details").className += " show";

            // Removes the hidden website form field to fight spam
            setTimeout(function () {
                var honeypot = document.querySelector("input[name=website]");
                honeypot.parentNode.removeChild(honeypot);
            }, 2000);
        }, false);
    }

    // Convert URL to links in comments
    var comments = document.querySelectorAll("#comments .content [itemprop=text]");

    requestAnimationFrame(function () {
        for (var i = 0; i < comments.length; i++) {
            var comment = comments[i];
            comment.innerHTML = urlify(comment.textContent);
        }
    });

    function urlify(text) {
        return text.replace(/(((https?:\/\/)|(www\.))[^\s]+)/g, function (url, b, c) {
            var url2 = c === 'www.' ? 'http://' + url : url;
            return '<a href="' + url2 + '" rel="nofollow noreferrer">' + url + '</a>';
        });
    }

    // Lazy load images/iframes
    window.addEventListener("load", function () {

        var timer,
            images,
            viewHeight;

        function init() {
            images = document.body.querySelectorAll("[data-src]");
            viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight);

            lazyload(0);
        }

        function scroll() {
            lazyload(200);
        }

        function lazyload(delay) {
            if (timer) {
                return;
            }

            timer = setTimeout(function () {
                var changed = false;

                requestAnimationFrame(function () {
                    for (var i = 0; i < images.length; i++) {
                        var img = images[i];
                        var rect = img.getBoundingClientRect();

                        if (!(rect.bottom < 0 || rect.top - 100 - viewHeight >= 0)) {
                            img.onload = function (e) {
                                e.target.className = "loaded";
                            };

                            img.className = "notloaded";
                            img.src = img.getAttribute("data-src");
                            img.removeAttribute("data-src");
                            changed = true;
                        }
                    }

                    if (changed) {
                        filterImages();
                    }

                    timer = null;
                });

            }, delay);
        }

        function filterImages() {
            images = Array.prototype.filter.call(
                images,
                function (img) {
                    return img.hasAttribute('data-src');
                }
            );

            if (images.length === 0) {
                window.removeEventListener("scroll", scroll);
                window.removeEventListener("resize", init);
                return;
            }
        }

        // polyfill for older browsers
        window.requestAnimationFrame = (function () {
            return window.requestAnimationFrame ||
                window.webkitRequestAnimationFrame ||
                window.mozRequestAnimationFrame ||
                function (callback) {
                    window.setTimeout(callback, 1000 / 60);
                };
        })();


        window.addEventListener("scroll", scroll);
        window.addEventListener("resize", init);

        init();
	});
	
	// Day / Night mode
	var nightModeClass = "night-mode";
	var nightModeCookie = "nightMode=1;"

	document.body.querySelector(".btn-day").addEventListener("click", function () {
		document.body.classList.remove(nightModeClass);
		document.cookie = nightModeCookie + " expires=Thu, 01 Jan 1970 00:00:01 GMT;"
	});
	document.body.querySelector(".btn-night").addEventListener("click", function () {
		document.body.classList.add(nightModeClass);
		document.cookie = nightModeCookie +  + " expires=Thu, 01 Jan 9999 00:00:01 GMT;"
	});

	//Navigation
	document.getElementById("menu-icon").addEventListener("click", function () {
		var navbar = document.body.querySelector(".navbar");		
		navbar.classList.toggle('expand');
    	return false;
	});

})(window, document);
