"use strict";

export class DirectoryChanged {

    Directory = "";
    Clipping = false;
    CursorX = 0;
    CursorY = 0;

    ConvertToObject() {
        return {
            Directory: this.Directory,
            Clipping: this.Clipping,
            CursorX: this.CursorX,
            CursorY: this.CursorY,
        }
    }

    static isDirectoryChanged(obj) {
        return (
            "Directory" in obj &&
            "Clipping" in obj &&
            "CursorX" in obj &&
            "CursorY" in obj
        );
    }

    static ConvertFromObject(data) {

        if (!DirectoryChanged.isDirectoryChanged(data)) {
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