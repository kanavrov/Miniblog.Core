(function () {
	var translationPrefix = "Frontend";
	var translationEndpoint = document.body.dataset.translationEndpoint;
	var translations = {};

	var request = new XMLHttpRequest();
	request.onreadystatechange = function () {
		if (this.readyState == 4 && this.status == 200) {
			translations = JSON.parse(this.responseText);
		}
	};
	request.open("GET", translationEndpoint + "?prefix=" + translationPrefix, true);
	request.send();

	window.i18n = function (key, defaultValue) {
		var formattedKey = translationPrefix + "." + key;

		if (translations.hasOwnProperty(formattedKey)) {
			return translations[formattedKey];
		}
		
		return defaultValue ? defaultValue : formattedKey;
	};

})(window, document);