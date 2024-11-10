// noinspection JSUnusedGlobalSymbols

"use strict";

export class Objects {

    /**
     * Merges the properties of a set of objects into a new object.
     * @param {...objects} objects
     * @returns {object} An object with all the properties of the supplied objects.
     */
    static Merge(...objects) {

        let newObject = {};

        for (let i= 0; i < objects.length; i++) {
            Object.assign(newObject, objects[i]);
        }

        return newObject;
    }

    /**
     * Creates a new object from an existing object using an entryMapper function to transform data.
     * @param {object} obj The object being "read".
     * @param {function(value, key, obj) } entryMapper A transformation function provided with the value to be converted, the key for the value, and a reference to the host object.
     * @returns {object} A new object with data transformed from the original object.
     * @constructor
     */
    static MapObject(obj, entryMapper) {
        return Object.fromEntries(
            Object.entries(obj).map(([key, value]) => [key, entryMapper(value, key, obj)])
        );
    }

    /**
     * Creates a clone of an object.
     * @param {object} obj
     * @returns {object} a copy of the object
     */
    static Clone(obj) {
        return JSON.parse(JSON.stringify(obj));
    }

    /**
     * Copies properties of a source object into a destination object, overwriting if requested.
     * @param {object} source - Object to be copied FROM.
     * @param {object} destination - Object to be copied TO.
     * @param {boolean} overwrite - Whether to overwrite properties in the destination if they already exist.
     */
    static CopyProperties(source, destination, overwrite) {

        overwrite = (overwrite === null ? false : overwrite);

        let clone = this.Clone(source);

        for (let propName in clone) {

            if (overwrite) {
                destination[propName] = clone[propName];
            } else {
                if (!(propName in destination)) {
                    destination[propName] = clone[propName];
                }
            }
        }
    }

    /**
     * Determines if an object has a value. If so, the object is returned. If not, the defaultValue is returned.
     * @param {object} obj - The object to be evaluated.
     * @param {object} defaultValue - The value to return if the object doesn't have a value.
     * @returns {object} The resulting value.
     */
    static ValueWithDefault(obj, defaultValue) {

        if (obj === null || obj === undefined) {
            return defaultValue;
        }

        return obj;
    }

    /**
     * Determines if an object has a value.
     * @param obj {object} The object to be evaluated.
     * @returns {boolean} Returns true if the object is not null and is not undefined.
     */
    static IsDefined(obj) {
        return obj !== null && obj !== undefined;
    }

    /**
     * Determines if an object's property has a value. If so, the value is returned. If not, the defaultValue is returned.
     * @param {object} obj - The object whose property is to be evaluated.
     * @param {string} propertyName - The property name of the property to be evaluated.
     * @param {object} defaultValue - The value to return if the property value isn't present.
     * @returns {object}
     */
    static PropertyValueWithDefault(obj, propertyName, defaultValue) {

        if (obj != null) {

            if (propertyName in obj) {

                let value = obj[propertyName];

                if (value != null && `${value}`.length > 0) {
                    return value;
                } else {
                    return defaultValue;
                }

            } else {
                return defaultValue;
            }

        } else {
            return defaultValue;
        }
    }

    /**
     * Provides an array of entries from an object sorted by the property specified.
     * @param {object} obj - The object from which to extract the array of entries.
     * @param {string} propertyName - The property to be used for the sort.
     * @returns An array of key/value pairs ordered by the property specified. Example: [[key1, value1], [key2, value2], [keyN, valueN]]
     */
    static SortObjectByProperty(obj, propertyName) {

        // noinspection JSUnusedLocalSymbols
        return Object.entries(obj).sort(
            // noinspection JSUnusedLocalSymbols
            function([a,b], [c,d]) {
                if (b[propertyName] > d[propertyName]) return 1;
                if (b[propertyName] < d[propertyName]) return -1;
                return 0;
            }
        );
    }

    /**
     * Provides a copy of an object with property names (keys) sorted. Example: {z: 1, y: 2, x: 3} becomes {x: 3, y: 2, z: 1}.
     * @param {object} obj - The object to be sorted.
     * @returns {{}}  - A copy of the object with properties sorted.
     */
    static SortObject(obj) {

        return Object.keys(obj).sort().reduce(
            function (result, key) {
                result[key] = obj[key];
                return result;
            },
            {}
        );
    };

    /**
     * Determines if a value is an object.
     * @param value The value to be evaluated.
     * @returns {boolean} Returns true only if the value's type is 'object'.
     * If the value is null or not an object, false is returned.
     */
    static isObject(value) {
        return typeof value === 'object' && value !== null;
    }

}