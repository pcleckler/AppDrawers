"use strict";

// noinspection JSUnusedGlobalSymbols
export class ItemTypes {

    static CaptureClip = new ItemTypes("CaptureClip", "", "", 0);

    static Clip = new ItemTypes(
        "Clip",
        ".Clip",
        "%SystemRoot%\\System32\\SHELL32.dll",
        260);

    static DateFormat = new ItemTypes(
        "DateFormat",
        ".DateFormat",
        "%SystemRoot%\\System32\\SHELL32.dll",
        167);

    static Directory = new ItemTypes(
        "Directory",
        "",
        "%SystemRoot%\\System32\\SHELL32.dll",
        3);

    static NewFolder = new ItemTypes("NewFolder", "", "", 0);
    static NewShortcut = new ItemTypes("NewShortcut", "", "", 0);
    static OpenFolder = new ItemTypes("OpenFolder", "", "", 0);
    static Shortcut = new ItemTypes("Shortcut", ".lnk", "", 0);

    DefaultIcon = "";
    DefaultIconIndex = 0;
    Extension = "";
    Type = "";

    constructor(type = "", extension = "", defaultIcon = "", defaultIconIndex = 0) {
        this.Type = type;
        this.Extension = extension;
        this.DefaultIcon = defaultIcon;
        this.DefaultIconIndex = defaultIconIndex;
    }

    /**
     * Converts the current object into a basic JavaScript object.
     * @returns {{}} A JavaScript interpretation of the current object.
     */
    ConvertToObject() {
        return {
            DefaultIcon: this.DefaultIcon,
            DefaultIconIndex: this.DefaultIconIndex,
            Extension: this.Extension,
            Type: this.Type,
        }
    }

    /**
     * Inspects a JavaScript object to determine if the current object's keys are present in the object. This implies that the JavaScript object can be converted into an instance of the current object model.
     * @param obj
     * @returns {boolean}
     */
    static CanConvert(obj) {
        return "DefaultIcon" in obj &&
            "DefaultIconIndex" in obj &&
            "Extension" in obj &&
            "Type" in obj
    }

    /**
     * Converts a JavaScript object into an instance of the current object model.
     * @param {object} data The JavaScript object to convert.
     * @returns {ItemTypes} An instance of the current object model.
     */
    static ConvertFrom(data) {

        if (!ItemTypes.CanConvert(data)) {
            throw "Unrecognized ItemTypes JSON.";
        }

        let it = new ItemTypes();

        it.DefaultIcon = data.DefaultIcon;
        it.DefaultIconIndex = data.DefaultIconIndex;
        it.Extension = data.Extension;
        it.Type = data.Type;

        return it;
    }
}