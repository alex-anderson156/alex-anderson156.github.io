app = angular.module('ResumeApp', ['ngMaterial', "chart.js"]);


app.controller('ResumeController', function () {
    vm = this;


    vm.activeCompetancy = {}

    vm.competancies = [
        {
            Title: "C# .NET",
            Background: "#690081",
            Foregound: 'white',
            Intro: "I have been working with .NET for over 9 years and have a comprehensive knowledge of both the language and the toolsets. ",

            Body: "Starting whilst studying at Sixth-Form, I used my initative to teach myself C# and learnt how to develop computer games using Microsofts XNA Toolkit." +
                " Whilst reading Computer Science at University I continued my exploration into C# and development using Window Presentation Foundation (WPF) to create desktop apps for personal use." +
                "" +
                "",

            Footer: "Working professionally has given me the further opportunity to expand my skills in Web development using ASP.NET while continuing to write and maintain many different applications, using various technologies." +
                "" +
                "" +
                "",

            Technologies: [ "WPF", "WCF", "SingalR", "ASP.NET", "Entity Framework"]
        },

        {
            Title: "Databases",
            Background: "#0072C6",
            Foregound: 'white',
            Intro: "Software and Databases go hand in hand, and since starting work professionally I have constantly interacted with SQL Databases, and become proficient in the querying and design of data storage solutions.",

            Body: "Working professionaly I have successfully designed and implemented two scalable relational database solutions in MSSQL, with related software engineered using A.C.I.D principals." +
                "" +
                "" +
                "",

            Footer: "I have taken a leading role in efforts to maximise the efficiency of our existing data stores, including rebuilding, and redesigning index structures, and optimising user queries" +
                "" +
                "" +
                "",

            Technologies: [ "SQL", "MSSQL", "T-SQL", "ORACLE", "NoSQL"]
        },

        {
            Title: "Software Architechture",
            Background: "#FFC000",
            Foregound: 'black',
            Intro: "Building software is great fun, maintaining software is hard ... unless the solution code is formatted and structured correctly.",

            Body: "One of my main passions within software development is the design of the solution, I use many different architchural design patterns to maximise the readability and reliability of code." +
                "This includes; The Repository Pattern, The Builder Patttern, The Factory Pattern, Object Pooling, The Flyweight Pattern and Dependancy Injection." +
                "" +
                "",

            Footer: "I recently created a 40,000 SLOC solution, built from the ground up, which was aided by gaining feedback from both users and other members of the development team. The solution provides a way for other developers to rapidly prototype and build applications." +
                "The software is designed to be modular, with an easy to use graphical interface heavily inspired by Google's Material Design to reduce the training required and increase development capability. " +
                "" +
                "",

            Technologies: [  ]
        },
    ]


    vm.setActiveCompetancy = function(index) {
        vm.activeCompetancy = vm.competancies[index];
        vm.activeCompetancyStyle = {'background-color': vm.activeCompetancy.Background , 'color': vm.activeCompetancy.Foregound };
    }



    vm.Technologies = [
        {
            "Icon" : "https://www.nop4you.com/content/images/thumbs/0001494_search-engine-powered-by-mongodb.jpeg",
            "Name": "Mongo DB",
            "Description": "I am currently researching NoSQL databases, through looking at MongoDB's application as an alternative style of database.",
        },

        {
            "Icon" : "http://www.wintellect.com/devcenter/wp-content/uploads/2015/07/angularjs_logo.svg_.png",
            "Name": "Angular 2",
            "Description": "I am currently investigating the changes to Angular 2, and upgrading our solution from Angular 1.6.*",
        },

        {
            "Icon" : "http://www.arduino.org/images/arduino_official_Logo__.png",
            "Name": "Arduino",
            "Description": "I am working on electrical engineering projects to compliment software development",
        },

        {
            "Icon" : "http://vignette3.wikia.nocookie.net/roblox/images/f/f4/Questionmark.png/revision/latest?cb=20120119031741",
            "Name": "New Technologies",
            "Description": "I am always on the look out for new technologies to explore, via news outlets such as Reddit and MSDN.",
        },
    ];

    // init
    vm.setActiveCompetancy(0);
});
