/**
 * @license Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.html or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
	// config.uiColor = '#AADC6E';
	config.pasteFromWordRemoveStyles = false;
	config.pasteFromWordRemoveFontStyles = false;
	//config.toolbarGroups = [
	//	//{ name: 'document', groups: ['mode', 'document', 'doctools'] },
	//	{ name: 'clipboard', groups: ['clipboard', 'undo'] },
	//	//{ name: 'editing', groups: ['find', 'selection', 'spellchecker'] },
	//	//{ name: 'forms' },
	//	{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
	//	{ name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'] },
	//	{ name: 'links' },
	//	{ name: 'insert' },
	//	{ name: 'styles' },
	//	{ name: 'colors' },
	//	{ name: 'tools' },
	//	{ name: 'others' },
	//	//{ name: 'about' }
	//];


	//use to configure the toolbaar of the ck editor
	config.toolbar =
	[
		////['Source', '-', 'Save', 'NewPage', 'Preview', '-', 'Templates'],
		['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Print'],//['SpellChecker', 'Scayt'],
		['Undo', 'Redo', '-', 'Find', 'Replace', '-', 'SelectAll', 'RemoveFormat'],
		//['Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton', 'HiddenField'],
		'/',//for lie break
		['Bold', 'Italic', 'Underline', 'Strike'], //['-', 'Subscript', 'Superscript'],
		['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent'],//['Blockquote', 'CreateDiv'],
		['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
		//['BidiLtr', 'BidiRtl'],
		['Link', 'Unlink', 'Anchor','Image'],
		//['Image', 'Flash', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'],
		//'/',
		['Styles', 'Format', 'Font', 'FontSize'],
		['TextColor', 'BGColor'],
		//['Maximize', 'ShowBlocks', '-', 'About']
	];
};
