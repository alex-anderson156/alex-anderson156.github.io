app = angular.module('ResumeApp', ['ngMaterial', "chart.js"]);


app.controller('ResumeController', function () {
    vm = this;


    vm.Technologies = [
        {
            "Icon" : "https://www.nop4you.com/content/images/thumbs/0001494_search-engine-powered-by-mongodb.jpeg",
            "Name": "Mongo DB",
            "Description": "Currently Investigating NoSQL Databases. Currently MongoDB",
        },

        {
            "Icon" : "http://www.wintellect.com/devcenter/wp-content/uploads/2015/07/angularjs_logo.svg_.png",
            "Name": "Angular 2",
            "Description": "Exploring The Angular JS Frameworks",
        },

        {
            "Icon" : "http://www.arduino.org/images/arduino_official_Logo__.png",
            "Name": "Arduino",
            "Description": "Investigating Electrical Engineering to compliment my Software",
        },

        {
            "Icon" : "http://vignette3.wikia.nocookie.net/roblox/images/f/f4/Questionmark.png/revision/latest?cb=20120119031741",
            "Name": "...",
            "Description": "I am always looking to Explore new technology, ",
        },
    ];

});
