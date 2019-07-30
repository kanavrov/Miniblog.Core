(function () {
	// File upload
	function handleFileSelect(file, imageUploadEndpoint, imageSizeLimit, onSuccess, onError) {
		if (window.File == null || window.FileList == null) {
			console.log(window.i18n("Post.FileApiError"));
			return;
		}

		if (!file.type.match("image"))
			return;

		if (getFileSizeInMegabytes(file) >= imageSizeLimit) {
			alert(window.i18n("Post.FileSizeError")
				.replace('#image_max_size#', imageSizeLimit)
			);
			return;
		}

		var formData = new FormData();
		formData.append("image", file);
		var request = new XMLHttpRequest();
		request.open("POST", imageUploadEndpoint);
		request.send(formData);

		request.onload = function () {
			if (this.status === 200) {
				if (onSuccess) {
					onSuccess(this.responseText, file);
				}
			} else {
				if (onError) {
					onError(this.status, this.statusText.toString());
				} else {
					alert(window.i18n("Post.FileUploadError"));
				}
			}
		};
	}

	function getFileSizeInMegabytes(file) {
		return file.size / 1048576.0;
	}

	function setupHtmlEditor(element) {
		if (typeof window.orientation !== "undefined" || navigator.userAgent.indexOf('IEMobile') !== -1) {
			tinymce.init({
				selector: '.html-editor',
				theme: 'modern',
				mobile: {
					theme: 'mobile',
					plugins: ['autosave', 'lists', 'autolink'],
					toolbar: ['undo', 'bold', 'italic', 'styleselect']
				}
			});
		} else {
			tinymce.init({
				selector: '.html-editor',
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
							browseImages(element, function (url, file) {
								afterImageUploadedHtml(editor, url, file);
							});
						}
					});
				}
			});
		}
	}

	function setupMarkdownEditor(element) {
		var editor = new EasyMDE({
			autoDownloadFontAwesome: false,
			autosave: {
				enabled: false,
				delay: 1000,
				uniqueId: 'md-editor-autosave'
			},
			toolbar: ["bold", "italic", "strikethrough", "heading-1", "heading-2", "heading-3", "|",
				"code", "quote", "unordered-list", "ordered-list", "|",
				{
					name: "insert-image",
					action: function customFunction(editor) {
						browseImages(element, function (url, file) {
							afterImageUploadedMarkdown(editor, url, file);
						});
					},
					className: "fa fa-image",
					title: "Insert Image",
				},
				"link", "table", 
				{
					name: "embed-video",
					action: function customFunction(editor) {
						onEmbedMedia(editor);
					},
					className: "fa fa-video",
					title: "Embed video",
				}, "|",
				"fullscreen", "|",
				"redo", "undo"],
			spellChecker: false,
			status: ["autosave", "lines", "words"],
			element: element
		});

		return editor;
	}

	function browseImages(editorElement, onSuccess, onError) {
		var editorContainer = editorElement.parentElement;
		var oldInput = editorContainer.querySelector(".image-input");
		var imageSizeLimit = editorElement.dataset.imageSizeLimitMegabytes;
		var imageUploadEndpoint = editorElement.dataset.imageUploadEndpoint;

		if (oldInput) {
			editorContainer.removeChild(oldInput);
		}

		var imageInput = document.createElement("input");
		imageInput.className = "image-input";
		imageInput.type = "file";
		imageInput.multiple = false;
		imageInput.name = "image";
		imageInput.accept = "image/*";
		imageInput.style.display = "none";
		imageInput.style.opacity = 0;

		editorElement.parentElement.appendChild(imageInput);

		imageInput.addEventListener("change", function (event) {
			if (event.target.files.length > 0) {
				handleFileSelect(event.target.files[0], imageUploadEndpoint, imageSizeLimit, onSuccess, onError);
			}
		});
		imageInput.click();
	}

	function afterImageUploadedMarkdown(editor, url, file) {
		var cm = editor.codemirror;
		var stat = editor.getState(cm);
		replaceSelectionMarkdown(editor, stat.image, ["![" + file.name + "](#url#)", ""], url);
	}

	function onEmbedMedia(editor) {
		var cm = editor.codemirror;
		var stat = editor.getState(cm);
		replaceSelectionMarkdown(editor, stat.image, ["![Video name](#url#)", ""], "");
	}

	function replaceSelectionMarkdown(editor, active, startEnd, url) {
		var cm = editor.codemirror;

		if (/editor-preview-active/.test(cm.getWrapperElement().lastChild.className))
			return;

		var text;
		var start = startEnd[0];
		var end = startEnd[1];
		var startPoint = {},
			endPoint = {};
		Object.assign(startPoint, cm.getCursor('start'));
		Object.assign(endPoint, cm.getCursor('end'));
		if (url) {
			end = end.replace('#url#', url);
			start = start.replace('#url#', url);
		}
		if (active) {
			text = cm.getLine(startPoint.line);
			start = text.slice(0, startPoint.ch);
			end = text.slice(startPoint.ch);
			cm.replaceRange(start + end, {
				line: startPoint.line,
				ch: 0,
			});
		} else {
			text = cm.getSelection();
			cm.replaceSelection(start + text + end);

			startPoint.ch += start.length;
			if (startPoint !== endPoint) {
				endPoint.ch += start.length;
			}
		}
		cm.setSelection(startPoint, endPoint);
		cm.focus();
	}

	function afterImageUploadedHtml(editor, url, file) {
		var image = new Image();
		image.alt = file.name;
		image.onload = function () {
			image.setAttribute("width", image.width);
			image.setAttribute("height", image.height);
			editor.execCommand('mceInsertContent', false, image.outerHTML);
		};
		image.src = url;
	}

	// edit form
	var edit = document.getElementById("edit");
	// Setup editor
	var htmlEditorElement = document.querySelector(".html-editor");
	var markdownEditorElement = document.querySelector(".md-editor");

	if (edit) {

		if (htmlEditorElement) {
			setupHtmlEditor(htmlEditorElement);
		}

		if (markdownEditorElement) {
			setupMarkdownEditor(markdownEditorElement);
		}

		// Delete post
		var deleteButton = edit.querySelector(".btn-delete");
		if (deleteButton) {
			deleteButton.addEventListener("click", function (e) {
				if (!confirm(window.i18n("Post.ConfirmDelete"))) {
					e.preventDefault();
				}
			});
		}
	}

	// Delete comments
	var deleteLinks = document.querySelectorAll("#comments a.btn-delete");
	if (deleteLinks) {
		for (var i = 0; i < deleteLinks.length; i++) {
			var link = deleteLinks[i];

			link.addEventListener("click", function (e) {
				if (!confirm(window.i18n("Comments.ConfirmDelete"))) {
					e.preventDefault();
				}
			});
		}
	}

	//Edit category
	var editCategory = document.getElementById("edit-category");
	if (editCategory) {
		// Delete post
		var deleteCategoryButton = editCategory.querySelector(".btn-delete");
		if (deleteCategoryButton) {
			deleteCategoryButton.addEventListener("click", function (e) {
				if (!confirm(window.i18n("Category.ConfirmDelete"))) {
					e.preventDefault();
				}
			});
		}
	}

	var categoryTable = document.querySelector(".category-table");
	if (categoryTable) {
		// Delete post
		var deleteCategoryButtons = categoryTable.querySelectorAll(".btn-delete");
		for (var i = 0; i < deleteCategoryButtons.length; i++) {
			deleteCategoryButtons[i].addEventListener("click", function (e) {
				if (!confirm(window.i18n("Category.ConfirmDelete"))) {
					e.preventDefault();
				}
			});
		}
	}

})();