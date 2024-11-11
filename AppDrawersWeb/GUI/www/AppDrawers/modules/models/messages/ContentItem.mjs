"use strict";

import {ItemTypes} from "../ItemTypes.mjs";

// noinspection JSUnusedGlobalSymbols
export class ContentItem {

    ImageUri = "";
    Target = null;
    Text = "";
    Type = null;

    /**
     * Converts the current object into a basic JavaScript object.
     * @returns {{}} A JavaScript interpretation of the current object.
     */
    ConvertToObject() {
        return {
            ImageUri: this.ImageUri,
            Target: this.Target,
            Text: this.Text,
            Type: this.Type,
        }
    }

    /**
     * Inspects a JavaScript object to determine if the current object's keys are present in the object. This implies that the JavaScript object can be converted into an instance of the current object model.
     * @param obj
     * @returns {boolean}
     */
    static CanConvert(obj) {
        return "ImageUri" in obj &&
            "Target" in obj &&
            "Text" in obj &&
            "Type" in obj
    }

    /**
     * Converts a JavaScript object into an instance of the current object model.
     * @param {object} data The JavaScript object to convert.
     * @returns {ContentItem} An instance of the current object model.
     */
    static ConvertFrom(data) {

        if (!ContentItem.CanConvert(data)) {
            throw "Unrecognized ContentItem JSON.";
        }

        if (!ItemTypes.CanConvert(data.Type)) {
            throw "Unrecognized ItemTypes JSON.";
        }

        let ci = new ContentItem();

        ci.ImageUri = data.ImageUri;
        ci.Target = data.Target;
        ci.Text = data.Text;
        ci.Type = ItemTypes.ConvertFrom(data.Type);

        return ci;
    }
}