"use strict";

import {HTML} from "../utilities/HTML.mjs";
import {DirectoryChanged} from "../models/messages/DirectoryChanged.mjs";
import {ContentItem} from "../models/messages/ContentItem.mjs";
import {MessageWrapper} from "../models/messages/MessageWrapper.mjs";
import {ItemTypes} from "../models/ItemTypes.mjs";
import {ApiServer} from "../controllers/ApiServer.mjs";

export class AppDrawer {

    #Element = null;
    #ParentElement = null;
    #Theme = null;
    #Cursor = null;
    #ItemPopups = [];
    #ActivePopup = null;
    #ActiveItem = null;

    constructor(parentElement, theme = null) {

        if (parentElement == null) {
            throw new Error("ParentElement element not provided");
        }

        this.#ParentElement = parentElement;

        if (theme == null) {
            this.#Theme = new ContentTheme();
        } else {
            this.#Theme = theme;
        }

        this.#Element = HTML.Create({
            tag: "div",
            style: {
                position: "absolute",
                left: 0,
                top: 0,
                right: 0,
                bottom: 0,
                display: "block",
                overflow: "hidden",
            },
            events: {
                click: (event) => {
                    ApiServer.HideMenu();
                },
                mousemove: (event) => {
                    this.#Cursor = {x: event.clientX, y: event.clientY};
                }
            }
        });

        this.#ParentElement.append(this.#Element);
    }

    ProcessMessage(message) {

        if (!MessageWrapper.CanConvert(message)) {
            return;
        }

        let msg = MessageWrapper.ConvertFrom(message);

        if (!DirectoryChanged.CanConvert(msg.Data)) {
            return;
        }

        // Clear all popups
        this.#Element.innerHTML = "";

        let dcMsg = DirectoryChanged.ConvertFrom(msg.Data);

        ApiServer.GetDirectoryContents(
            dcMsg.Directory,
            (message) => {

                if (!Array.isArray(message.Data)) {
                    return;
                }

                this.#DisplayDirectory(message.Data, dcMsg.CursorX, dcMsg.CursorY, 0, 0);

                ApiServer.DisplayMenu();
            },
        )
    }

    /**
     * Moves an element to the requested location.
     * @param {HTMLElement} popupElement The element to be moved.
     * @param {object} anchorRect The rectangle defining the approximate location to which the element should be moved. The element will be placed in such a way as to remain on-screen.
     * @param {int} anchorRect.x The left coordinate of the rectangle.
     * @param {int} anchorRect.y The top coordinate of the rectangle.
     * @param {int} anchorRect.width The width of the rectangle.
     * @param {int} anchorRect.height The height of the rectangle.
     */
    #DisplayPopup({popupElement, anchorRect}) {

        // Default coordinates to the top-right of the requested rectangle.
        let bounds = {
            left: anchorRect.x + anchorRect.width,
            top: anchorRect.y - anchorRect.height,
            width: popupElement.offsetWidth,
            height: popupElement.offsetHeight,
        }

        // Determine if coordinates and size of popupElement will clip
        if ((bounds.left + popupElement.offsetWidth) > this.#Element.offsetWidth) {
            bounds.left = anchorRect.x - popupElement.offsetWidth;
        }

        if (bounds.left < 0) {
            bounds.width = popupElement.offsetWidth + bounds.left;
        }

        if ((bounds.top + popupElement.offsetHeight) > this.#Element.offsetHeight) {
            bounds.top = anchorRect.y - popupElement.offsetHeight;
        }

        if (bounds.top < 0) {
            bounds.height = popupElement.offsetHeight + bounds.top;
        }

        // console.log("AnchorRect:", anchorRect);
        // console.log("PopupElement:", {width: popupElement.offsetWidth, height: popupElement.offsetHeight});
        // console.log("this.#Element:", {left: this.#Element.offsetLeft, top: this.#Element.offsetTop, width: this.#Element.offsetWidth, height: this.#Element.offsetHeight});
        // console.log("Bounds:", bounds);

        // Move the popup's coordinates
        HTML.SetStyle(popupElement, {
            left: `${bounds.left}px`,
            top: `${bounds.top}px`,
            width: `${bounds.width}px`,
            height: `${bounds.height}px`,
        })
    }

    #GetRandomId() {
        return `id-${Date.now()}-${Math.floor(Math.random() * 1000)}`;
    }

    #DisplayDirectory(itemList, cursorX, cursorY, width, height) {

        let menuElement = null;
        let itemElements = [];

        let highlightElement = (element) => {

            while (this.#ItemPopups.length > 0) {

                let itemPopup = this.#ItemPopups.pop();

                if (!(this.#ActivePopup != null && itemPopup.id === this.#ActivePopup.id)) {
                    this.#Element.removeChild(itemPopup);
                }
            }

            if (this.#ActivePopup != null) {
                this.#ItemPopups.push(this.#ActivePopup);
            }

            for (let i = 0; i < itemElements.length; i++) {
                HTML.SetStyle(itemElements[i], {
                    "background-color": "",
                    "box-shadow": "",
                });
            }

            if (element) {
                HTML.SetStyle(element, {
                    "background-color": "rgba(255, 255, 255, 0.1)",
                    "box-shadow": "0px 2px 4px rgba(0, 0, 0, 0.3)",
                });
            }
        }

        this.#Element.append(HTML.Create({
            tag: "div",
            attributes: {
                id: this.#GetRandomId(),
            },
            style: {
                display: "block",
                "background-color": "rgba(40, 40, 40, 0.85)",
                "border-radius": "5px",
                "box-shadow": "0px 4px 8px rgba(0, 0, 0, 0.5)",
                "overflow-y": "auto",
                padding: "5px",
                position: "absolute",
                "scrollbar-width": "thin",
                "scrollbar-color": "rgb(255, 255, 255, 0.1) transparent",
            },
            inlineModifier: (element) => {
                menuElement = element;
            },
        }));

        for (let i = 0; i < itemList.length; i++) {

            if (!ContentItem.CanConvert(itemList[i])) {
                continue;
            }

            let item = ContentItem.ConvertFrom(itemList[i]);

            menuElement.append(HTML.Create({
                tag: "div",
                attributes: {
                    id: this.#GetRandomId(),
                },
                style: {
                    display: "block",
                },
                inlineModifier: (element) => {
                    itemElements.push(element);
                },
                events: {
                    mouseover: (event) => {

                        let item = itemList[i];

                        let itemElement = itemElements[i];

                        if (item.Type.Type === ItemTypes.Directory.Type && (this.#ActiveItem === null || (this.#ActiveItem.id !== itemElement.id))) {
                            
                            ApiServer.GetDirectoryContents(
                                item.Target,
                                (message) => {

                                    if (!Array.isArray(message.Data)) {
                                        return;
                                    }

                                    let itemPopup = this.#DisplayDirectory(message.Data, event.clientX, event.clientY, itemElement.offsetWidth, itemElement.offsetHeight);

                                    this.#ItemPopups.push(itemPopup);

                                    this.#ActivePopup = itemPopup;
                                },
                            )

                            this.#ActiveItem = itemElement;
                        }

                        highlightElement(itemElement); // Logical mapping between the itemElements array and the itemList array. Formerly event.target.
                    }
                },
                children: [
                    HTML.Create({
                        tag: "div",
                        style: {
                            display: "block",
                            "white-space": "nowrap",
                        },
                        events: {
                            click: () => {
                                this.#ItemClicked(item);
                            },
                        },
                        children: [
                            HTML.Create({
                                tag: "div",
                                style: {
                                    display: "flex",
                                    "align-items": "center",
                                },
                                children: [
                                    HTML.Create({
                                        tag: "img",
                                        attributes: {
                                            src: item.ImageUri,
                                        },
                                        style: {
                                            width: "16px",
                                            padding: "2px",
                                        }
                                    }),
                                    HTML.Create({
                                        tag: "span",
                                        properties: {
                                            innerText: item.Text
                                        },
                                        style: {
                                            padding: "2px",
                                        }
                                    })
                                ]
                            }),
                        ],
                    })
                ]
            }));
        }

        this.#DisplayPopup({popupElement: menuElement, anchorRect: {x: cursorX, y: cursorY, width: width, height: height}});

        return menuElement;
    }

    #ItemClicked(item) {
        alert(item.Text);
        ApiServer.HideMenu();
    }
}

export class ContentTheme {

}