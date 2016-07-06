var assert = require('chai').assert;

var vs_repl = require('../visualstudio_nodejs_repl');
var util = require('util');

function assertResult(expected, input) {
    var result = vs_repl.processRequest({
        type: "execute",
        code: input,
    });
    assert.equal("execute", result.type)
    assert.equal(undefined, result.error)
    assert.equal(util.inspect(expected, undefined, undefined, true), result.result)
};

describe('Repl Tests', function () {
    describe('First test', function () {
        it('Should not crash for empty request', function () {
            vs_repl.processRequest({});
        });
    });

    describe('Execute expression test', function () {
        it('Should return formatted string for number literal', function () {
            assertResult(1, '1');
        });

        it('Should return formatted string for null literal', function () {
            assertResult(null, 'null');
        });

        it('Should return formatted string of undefined for function', function () {
            assertResult(undefined, 'function a() { }');
        });

        it('Should evaluate function and return result', function () {
            assertResult(1, '(function() { return 1; }())');
        });
    });

    describe('Execute multiple statements tests', function () {
        it('Should return formatted string for number literal', function () {
            assertResult(2, '1; 2;');
        });

        it('Should allow variable assignment', function () {
            assertResult(undefined, 'var a = 2;');
            assertResult(2, 'a');
        });
    });

    describe('Use global variable tests', function () {
        it('Should allow the use of global variables', function () {
            assertResult("function", "typeof require");
            vs_repl.processRequest({
                type: "clear",
            });
            assertResult("function", "typeof require");
        });

        it('Should allow variable assignment', function () {
            assertResult(undefined, 'var a = 2;');
            assert.equal(undefined, global.a);
        });
    });

    describe('Clear tests', function () {
        it('Should allow variable assignment', function () {
            assertResult(undefined, 'var a = 2;');
            vs_repl.processRequest({
                type: "clear",
            });
            assertResult("undefined", 'typeof a');
        });
    });
}); 