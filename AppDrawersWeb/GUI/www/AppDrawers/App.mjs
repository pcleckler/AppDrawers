"use strict";

import {Resources} from "./modules/models/Resources.mjs";
import {AppDrawer} from "./modules/controls/AppDrawer.mjs";

export class App {

    #AppDrawer = null;

    constructor() {

        document.title = Resources.AppTitle;

        this.#AppDrawer = new AppDrawer(document.body);
    }

    ProcessNativeMessage(message) {

        this.#AppDrawer.ProcessMessage(message);

        return true;
    }
}

window.app = new App();

function ProcessNativeMessage(message) {
    window.app.ProcessNativeMessage(message);
}