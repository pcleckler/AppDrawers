"use strict";

import {MessageWrapper} from "../models/messages/MessageWrapper.mjs";

export class ApiServer {

    /**
     * Gets the contents of a directory.
     * @param {string} directory The requested directory.
     * @param {function(MessageWrapper)} success A callback for a successful retrieval.
     * @param {function(Response)} failure A callback for a failed retrieval.
     */
    static GetDirectoryContents(directory, success = null, failure = null) {

        fetch(`./getDirectoryContents?dir=${directory}`)

            .then((response) => {
                if (response.ok) {
                    return response.json();
                }
            })

            .then((message) => {

                if (!MessageWrapper.CanConvert(message)) {
                    return;
                }

                let msg = MessageWrapper.ConvertFrom(message);

                if (success) {
                    success(msg);
                }
            })

            .catch((reason) => {
                if (failure) {
                    failure(reason)
                }
            })
    }

    static DisplayMenu() {
        fetch("./displayMenu").then();
    }

    static HideMenu() {
        fetch("./hideMenu").then();
    }

}