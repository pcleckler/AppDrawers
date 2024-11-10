"use strict";

import {HTML} from "../utilities/HTML.mjs";
import {DirectoryChanged} from "../models/messages/DirectoryChanged.mjs";

export class AppDrawer {

    #Element = null;
    #ParentElement = null;
    #Theme = null;

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
                // border: "2px solid green",
            },
            events: {
                click: (e) => {
                    this.#SendHideRequest();
                }
            }
        });

        this.#ParentElement.append(this.#Element);
    }

    ProcessMessage(message) {

        if (!DirectoryChanged.isDirectoryChanged(message.Data)) {
            return;
        }

        let dcMsg = DirectoryChanged.ConvertFromObject(message.Data);

        fetch(`./getDirectoryContents?dir=${dcMsg.Directory}`)

            .then((response) => {
                if (response.ok) {
                    return response.json();
                }
            })

            .then((data) => {

                // Load menu
                this.#Load(data, dcMsg.CursorX, dcMsg.CursorY);

                fetch("./displayMenu").then();

            })

            .catch((response) => {
                //element.innerText = `API Server Version is not available.`
            })
    }



    #DisplayPopup(popupElement, cursorX, cursorY) {

        HTML.SetStyle(popupElement, {
            left: `${cursorX}px`,
            top: `${cursorY}px`,
        })
    }

    #Load(message, cursorX, cursorY) {

        this.#Element.innerHTML = "";

        let menuElement = null;
        let itemElements = [];

        function highlightElement(element) {

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
            style: {
                display: "block",
                // border: "1px solid blue",
                "background-color": "rgba(40, 40, 40, 0.85)",
                "border-radius": "5px",
                "box-shadow": "0px 4px 8px rgba(0, 0, 0, 0.5)",
                "overflow-y": "auto",
                padding: "5px",
                position: "absolute",
                "max-height": "99vh",
                "max-width": "99vw",
                "scrollbar-width": "thin",
                "scrollbar-color": "rgb(255, 255, 255, 0.1) transparent",
            },
            inlineModifier: (element) => {
                menuElement = element;
            },
        }));

        for (let i = 0; i < message.Data.length; i++) {

            let item = message.Data[i];

            menuElement.append(HTML.Create({
                tag: "div",
                style: {
                    display: "block",
                },
                inlineModifier: (element) => {
                    itemElements.push(element);
                },
                events: {
                    mouseover: (event) => {
                        highlightElement(itemElements[i]); // Logical mapping between the itemElements array and the message.Data array. Formerly event.target.
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
                            click: (event) => {
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

        this.#DisplayPopup(menuElement, cursorX, cursorY)
    }

    #ItemClicked(item) {
        alert(item.Text);
        this.#SendHideRequest();
    }

    #SendHideRequest() {

        window.scrollTo({top: 0, left: 0, behavior: "instant"});

        fetch("./hideMenu").then();
    }
}

export class ContentTheme {

}