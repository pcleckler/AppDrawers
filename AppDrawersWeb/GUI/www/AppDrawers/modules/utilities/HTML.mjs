// noinspection JSUnusedGlobalSymbols

"use strict";

import {Objects} from "./Objects.mjs";

export class HTML {

    /**
     * Pass-thru for document.createTextNode(). Creates a new Text node. This method can be used to escape HTML characters.
     * @param data A string containing the data to be put in the text node.
     * @returns {Text} A Text node.
     */
    static CreateTextNode(data) {
        return document.createTextNode(data);
    }

    /**
     * Gets the value of a CSS variable by name from the computed style of the page.
     * @param {string} variableName The name of the CSS variable to obtain. Must include the '--' prefix.
     * @returns {string}
     */
    static GetCssVariable(variableName) {
        return getComputedStyle(document.documentElement).getPropertyValue(variableName);
    }

    /**
     * Sets the style of an element using the supplied style object. NOTE: Existing styles will be maintained if not overridden by the style object provided.
     * @param {HTMLElement} element The HTML element whose style will be modified.
     * @param {Object.<string, string>} style The style properties to be merged into the element's style.
     */
    static SetStyle(element, style) {

        let styleRecords = Objects.ValueWithDefault(element.getAttribute("style"), "").split(";");

        // Load Style Object
        let styleObject = {};

        for (let i = 0; i < styleRecords.length; i++) {

            if (styleRecords[i].trim().length < 1) continue;

            let propertyName = "";
            let value = "";

            let tokens = styleRecords[i].split(":");

            for (let tokenIndex = 0; tokenIndex < tokens.length; tokenIndex++) {

                if (propertyName.length < 1) {
                    propertyName = tokens[tokenIndex].replace(/\s/g, " ").trim();
                } else {
                    value = tokens[tokenIndex].trim();
                    break;
                }
            }

            if (propertyName.length > 0) {
                styleObject[propertyName] = value;
            }
        }

        // Modify Style Attributes
        for (let propertyName in style) {
            styleObject[propertyName] = style[propertyName];
        }

        // Load new styles into element

        let newStyleEntries = [];

        for (let propertyName in styleObject) {
            newStyleEntries.push(`${propertyName}: ${styleObject[propertyName]}`);
        }

        element.setAttribute("style", newStyleEntries.join(";"));
    }

    /**
     * Creates an element from the specified HTML string. Note: the element is not added to the document.
     * @param {string} htmlString
     * @returns {Element}
     */
    static CreateFromHtml(htmlString) {

        let tempDiv = document.createElement('div');

        tempDiv.innerHTML = htmlString.trim();

        return tempDiv.firstElementChild;
    }

    /**
     * Creates an HTML element for the specified tag. Note: the element is not added to the document.
     * @param {string} tag The HTML element type (tag) to be created.
     * @param {Object.<string, object>} attributes An object whose keys will be used to set attributes of the element, such as HREF or SRC. Note that a Style attribute can be passed in as an object, but all other attributes will be handled as strings.
     * @param {Object.<string, any>} style An object whose keys will be used to set style declarations of the element. This parameter can be included in the attributes object, and if style declarations are specified here and also in the attributes parameter, the style declarations will be merged, with the `style` parameter's declarations taking priority.
     * @param {Object.<string, object>} properties An object whose keys will be used to set properties of the element, such as innerHTML or innerText.
     * @param {Array.<HTMLElement>} children An array of HTMLElements which will be registered as child elements for the new element.
     * @param {HTMLElementEventMap} events An object whose keys will be used to create event listeners for the new element.
     * @param {function(HTMLElement)} inlineModifier A callback allowing custom in-line modification of the element. One example use is to grab a reference to the specific element rather than having to create the element externally and pass it in.
     * @returns {HTMLElement}
     */
    static Create({tag, attributes = null, style = null, properties = null, children = null, events = null, inlineModifier = null}) {

        const styleKey = "style";

        // Provide Default Parameters
        if (attributes === undefined || attributes == null) {
            attributes = {};
        }

        if (!(style === undefined || style == null)) {

            // If present in attributes as well, merge Style object into Attributes Style object
            if (styleKey in attributes) {

                attributes[styleKey] = HTML.StyleRuleToObject(HTML.ObjectToStyleRule(attributes[styleKey]));

                Object.assign(attributes[styleKey], style);
            } else {
                attributes[styleKey] = style;
            }
        }

        if (properties === undefined || properties == null) {
            properties = {};
        }

        if (children === undefined || children == null) {
            children = [];
        }

        if (events === undefined || events == null) {
            // noinspection JSValidateTypes
            events = {};
        }

        // Generate element and set configuration
        let element = document.createElement(tag);

        for (let attribName in attributes) {

            if (attribName.toLowerCase() === styleKey) {
                element.setAttribute(attribName, HTML.ObjectToStyleRule(attributes[attribName]));
            } else {
                element.setAttribute(attribName, `${attributes[attribName]}`);
            }
        }

        for (let propName in properties) {
            element[propName] = properties[propName];
        }

        for (let childIndex = 0; childIndex < children.length; childIndex++) {
            if (children[childIndex] != null) {
                element.append(children[childIndex]);
            }
        }

        for (let eventName in events) {
            element.addEventListener(eventName, events[eventName]);
        }

        if (inlineModifier !== undefined && inlineModifier !== null) {
            inlineModifier(element);
        }

        return element;
    }

    /**
     * Converts a Style-rule string into an object representing Style declarations.
     * @param styleString The style-rule string containing styling declarations.
     * @returns {{}} An object representing Style declarations.
     */
    static StyleRuleToObject(styleString) {

        let styleObj = {};

        let declarationList = styleString.split(";");

        for (let i = 0; i < declarationList.length; i++) {

            let key   = "";
            let value = "";

            let tokenList = declarationList[i].split(":");

            for (let j = 0; j < tokenList.length; j++) {

                if (key.length < 1) {
                    key = tokenList[j].trim().toLowerCase();
                } else {
                    value = tokenList[j].trim();
                    break;
                }
            }

            styleObj[key] = value;
        }

        return styleObj;
    }

    /**
     * Converts an object representing Style declarations into a Style-rule string.
     * @param styleObj The object containing style declarations. If this parameter is not an object, the parameter is returned immediately, with the assumption it is already a string formatted as a style rule.
     * @returns {*|string} A style-rule string consisting of style declarations separated by semicolons.
     */
    static ObjectToStyleRule(styleObj) {

        if (!Objects.isObject(styleObj)) return styleObj;

        let sb = [];

        for (let styleAttrib in styleObj) {
            sb.push(`${styleAttrib}: ${styleObj[styleAttrib]};`);
        }

        return sb.join(" ");
    }

    /**
     * Gets the minimum and maximum numeric z-indexes of a parent element's children using computed styles.
     * @param parentElement The parent element to evaluate for z-indexes.
     * @returns {{min: number, max: number}}
     */
    static GetZIndexRange(parentElement) {

        const InitialMinZIndex = 32000;
        const InitialMaxZIndex = -32000;

        let minZIndex = InitialMinZIndex;
        let maxZIndex = InitialMaxZIndex;
        let allElements = parentElement.getElementsByTagName('*');

        for (let i = 0; i < allElements.length; i++) {

            let zIndex = parseFloat(window.getComputedStyle(allElements[i]).zIndex);

            if (!isNaN(zIndex) && zIndex < minZIndex) {
                minZIndex = zIndex;
            }

            if (!isNaN(zIndex) && zIndex > maxZIndex) {
                maxZIndex = zIndex;
            }
        }

        if (minZIndex === InitialMinZIndex) {
            minZIndex = 0;
        }

        if (maxZIndex === InitialMaxZIndex) {
            maxZIndex = 0;
        }

        return {min: minZIndex, max: maxZIndex};
    }
}