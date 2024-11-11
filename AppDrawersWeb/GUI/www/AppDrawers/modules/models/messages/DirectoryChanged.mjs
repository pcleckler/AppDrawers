"use strict";

// noinspection JSUnusedGlobalSymbols
export class DirectoryChanged {

    Directory = "";
    Clipping = false;
    CursorX = 0;
    CursorY = 0;

    /**
     * Converts the current object into a basic JavaScript object.
     * @returns {{}} A JavaScript interpretation of the current object.
     */
    ConvertToObject() {
        return {
            Directory: this.Directory,
            Clipping: this.Clipping,
            CursorX: this.CursorX,
            CursorY: this.CursorY,
        }
    }

    /**
     * Inspects a JavaScript object to determine if the current object's keys are present in the object. This implies that the JavaScript object can be converted into an instance of the current object model.
     * @param obj
     * @returns {boolean}
     */
    static CanConvert(obj) {
        return (
            "Directory" in obj &&
            "Clipping" in obj &&
            "CursorX" in obj &&
            "CursorY" in obj
        );
    }

    /**
     * Converts a JavaScript object into an instance of the current object model.
     * @param {object} data The JavaScript object to convert.
     * @returns {DirectoryChanged} An instance of the current object model.
     */
    static ConvertFrom(data) {

        if (!DirectoryChanged.CanConvert(data)) {
            throw "Unrecognized DirectoryChanged JSON.";
        }

        let dc= new DirectoryChanged();

        dc.Directory = data.Directory;
        dc.Clipping = data.Clipping === "true";
        dc.CursorX = data.CursorX;
        dc.CursorY = data.CursorY;

        return dc;
    }
}