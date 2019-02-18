(function () {

    // File upload
    function handleFileSelect(event) {
        if (window.File && window.FileList && window.FileReader) {

            var files = event.target.files;

            for (var i = 0; i < files.length; i++) {
                var file = files[i];

                // Only image uploads supported
                if (!file.type.match('image'))
                    continue;

                var reader = new FileReader();
                reader.addEventListener("load", function (event) {
                    var image = new Image();
                    image.alt = file.name;
                    image.onload = function (e) {
                        image.setAttribute("data-filename", file.name);
                        image.setAttribute("width", image.width);
                        image.setAttribute("height", image.height);
                        tinymce.activeEditor.execCommand('mceInsertContent', false, image.outerHTML);
                    };
                    image.src = this.result;

                });

                reader.readAsDataURL(file);
            }

            document.body.removeChild(event.target);
        }
        else {
            console.log("Your browser does not support File API");
        }
    }

    // edit form
    var edit = document.getElementById("edit");
    // Setup editor
    var editPost = document.getElementById("Content");

    if (edit && editPost) {

        if (typeof window.orientation !== "undefined" || navigator.userAgent.indexOf('IEMobile') !== -1) {
            tinymce.init({
                selector: '#Content',
                theme: 'modern',
                mobile: {
                    theme: 'mobile',
                    plugins: ['autosave', 'lists', 'autolink'],
                    toolbar: ['undo', 'bold', 'italic', 'styleselect']
                }
            });
        } else {
            tinymce.init({
                selector: '#Content',
                autoresize_min_height: 200,
                plugins: 'autosave preview searchreplace visualchars image link media fullscreen code codesample table hr pagebreak autoresize nonbreaking anchor insertdatetime advlist lists textcolor wordcount imagetools colorpicker',
                menubar: "edit view format insert table",
                toolbar1: 'formatselect | bold italic blockquote forecolor backcolor | imageupload link | alignleft aligncenter alignright  | numlist bullist outdent indent | fullscreen',
                selection_toolbar: 'bold italic | quicklink h2 h3 blockquote',
                autoresize_bottom_margin: 0,
                paste_data_images: true,
                image_advtab: true,
                file_picker_types: 'image',
                relative_urls: false,
                convert_urls: false,
                branding: false,

                setup: function (editor) {
                    editor.addButton('imageupload', {
                        icon: "image",
                        onclick: function () {
                            var fileInput = document.createElement("input");
                            fileInput.type = "file";
                            fileInput.multiple = true;
                            fileInput.accept = "image/*";
                            fileInput.addEventListener("change", handleFileSelect, false);
                            fileInput.click();
                        }
                    });
                }
            });
        }

        // Delete post
        var deleteButton = edit.querySelector(".delete");
        if (deleteButton) {
            deleteButton.addEventListener("click", function (e) {
                if (!confirm("Are you sure you want to delete the post?")) {
                    e.preventDefault();
                }
            });
        }
    }

    // Delete comments
    var deleteLinks = document.querySelectorAll("#comments a.delete");
    if (deleteLinks) {
        for (var i = 0; i < deleteLinks.length; i++) {
            var link = deleteLinks[i];

            link.addEventListener("click", function (e) {
                if (!confirm("Are you sure you want to delete the comment?")) {
                    e.preventDefault();
                }
            });
        }
	}
	
	//Edit category
	var editCategory = document.getElementById("edit-category");
	if(editCategory) {
		// Delete post
        var deleteCategoryButton = editCategory.querySelector(".delete");
        if (deleteCategoryButton) {
            deleteCategoryButton.addEventListener("click", function (e) {
                if (!confirm("Are you sure you want to delete the category?")) {
                    e.preventDefault();
                }
            });
        }
	}

	var categoryTable = document.querySelector(".category-table");
	if(categoryTable) {
		// Delete post
		var deleteCategoryButtons = categoryTable.querySelectorAll(".delete");
		for(var i = 0; i < deleteCategoryButtons.length; i++) {
			deleteCategoryButtons[i].addEventListener("click", function (e) {
                if (!confirm("Are you sure you want to delete the category?")) {
                    e.preventDefault();
                }
            });
		}
	}

})();