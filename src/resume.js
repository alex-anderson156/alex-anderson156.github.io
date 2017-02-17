app = angular.module('ResumeApp', ['ngMaterial', "chart.js"]);


app.controller('ResumeController', function () {
    vm = this;


    vm.activeCompetancy = {}

    vm.competancies = [
        {
            Title: "C# .NET",
            Background: "#690081",
            Intro: "I have been Worked with .NET for over 9 years and have become incredibally familiar with both the language and the toolsets. ",

            Body: "Starting while Studying at Sixth-form, I used my initative to teach myself C# and learnt how to develop Computer games using Microsofts XNA Toolkit." +
                " While reading at University I continued my exploration into C# and development using Window Presentation Foundation (WPF) to creat desktop apps for personal use." +
                "" +
                "",

            Footer: "Working professionally has given me the oppertunity to further my skills in Web development using ASP.NET while continuing to write and maintain many differnt applications, using various technologies" +
                "" +
                "" +
                "",

            Technologies: [ "WPF", "WCF", "SingalR", "ASP.NET", "Entity Framework"]
        },

        {
            Title: "Databases",
            Background: "#0072C6",
            Intro: "Software and Databases go Hand in Hand, and since starting work professionally i have constantly interacted with SQL Databases, and have become very proficent in the quering and design of Data Storage Solutions.",

            Body: "" +
                "" +
                "" +
                "",

            Footer: "" +
                "" +
                "" +
                "",

            Technologies: [ "SQL", "MSSQL", "T-SQL", "ORACLE", "NoSQL"]
        },


    ]


    vm.setActiveCompetancy = function(index) {
        vm.activeCompetancy = vm.competancies[index];
        vm.activeCompetancyStyle = {'background-color': vm.activeCompetancy.Background , 'color': 'white' };
    }



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

    // init
    vm.setActiveCompetancy(0);
});
