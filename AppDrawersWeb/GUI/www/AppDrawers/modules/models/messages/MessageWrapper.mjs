"use strict";

import {ItemTypes as messages} from "../ItemTypes.mjs";

export class MessageWrapper {

    Type = "";
    Data = null;

    /**
     * Converts the current object into a basic JavaScript object.
     * @returns {{}} A JavaScript interpretation of the current object.
     */
    ConvertToObject() {
        return {
            Type: this.Type,
            Data: this.Data,
        }
    }

    /**
     * Inspects a JavaScript object to determine if the current object's keys are present in the object. This implies that the JavaScript object can be converted into an instance of the current object model.
     * @param obj
     * @returns {boolean}
     */
    static CanConvert(obj) {
        return "Type" in obj &&
            "Data" in obj
    }

    /**
     * Converts a JavaScript object into an instance of the current object model.
     * @param {object} data The JavaScript object to convert.
     * @returns {MessageWrapper} An instance of the current object model.
     */
    static ConvertFrom(data) {

        if (!MessageWrapper.CanConvert(data)) {
            throw "Unrecognized MessageWrapper JSON";
        }

        let mw = new MessageWrapper();

        mw.Type = data.Type;
        mw.Data = data.Data;

        return mw;
    }
}