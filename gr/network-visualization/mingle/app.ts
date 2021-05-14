
function descriptor(object) {
    var desc = {}, p;
    for (p in object) {
        desc[p] = {
            value: object[p],
            writable: true,
            enumerable: true,
            configurable: true
        };
    }
    return desc;
}

function merge(proto, object) {
    return Object.create(proto, descriptor(object));
}

function extend(proto, object) {
    var p;
    for (p in object) {
        proto[p] = object[p];
    }
}