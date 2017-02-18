app = angular.module('ResumeApp', ['ngMaterial', "chart.js"]);


app.controller('ResumeController', function () {
    vm = this;


    vm.activeCompetancy = {}

    vm.competancies = [
        {
            Title: "C# .NET",
            Background: "#690081",
            Foregound: 'white',
            Intro: "I have been Worked with .NET for over 9 years and have become incredibally familiar with both the language and the toolsets. ",

            Body: "Starting while Studying at Sixth-form, I used my initative to teach myself C# and learnt how to develop Computer games using Microsofts XNA Toolkit." +
                " While reading at University I continued my exploration into C# and development using Window Presentation Foundation (WPF) to create desktop apps for personal use." +
                "" +
                "",

            Footer: "Working professionally has given me the oppertunity to further my skills in Web development using ASP.NET while continuing to write and maintain many differnt applications, using various technologies." +
                "" +
                "" +
                "",

            Technologies: [ "WPF", "WCF", "SingalR", "ASP.NET", "Entity Framework"]
        },

        {
            Title: "Databases",
            Background: "#0072C6",
            Foregound: 'white',
            Intro: "Software and Databases go Hand in Hand, and since starting work professionally I have constantly interacted with SQL Databases, and have become very proficent in the querying and design of Data Storage Solutions.",

            Body: "Working Professionaly I have successfully designed and implemented two scalable relational database solutions in MSSQL, with related software engineered using A.C.I.D principals." +
                "" +
                "" +
                "",

            Footer: "I have taken a Leading Role in Efforts to Maximise the efficiency of our exising data stores, including rebuilding, and redesigning Index Structures, and Optimising User Queries" +
                "" +
                "" +
                "",

            Technologies: [ "SQL", "MSSQL", "T-SQL", "ORACLE", "NoSQL"]
        },

        {
            Title: "Software Architechture",
            Background: "#FFC000",
            Foregound: 'black',
            Intro: "Building Software is Great fun, Maintaining Software is Hard ... unless the Solution code is Formatted and structured correctly.",

            Body: "One of my Main passions within software development is the design of the Solution, I use many Different Architchural design patterns to maximise the Readability and Reliability of code." +
                "including; The Repository Pattern, The Builder Patttern, The Factory Pattern, Object Pooling, The Flyweight Pattern and Dependancy Injection." +
                "" +
                "",

            Footer: "I have recently created a 40,000 SLOC Solution, built from the ground up. Listenting and taking Feedback from both users and other members of the development team, The solution provides a way for other developers to rapidly prototype and build Applications, which can be easily distibuted to all users." +
                "The Software was design to be modular, with an use to use Graphical Interface heavily inspired by Googles Material Design to reduce the training required and increase development capability." +
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
            "Description": "I am Currently Investigating NoSQL Databases, Currently looking at MongoDB's application as an alternative style of database.",
        },

        {
            "Icon" : "http://www.wintellect.com/devcenter/wp-content/uploads/2015/07/angularjs_logo.svg_.png",
            "Name": "Angular 2",
            "Description": "I am Currently investigating the changes to Angular 2, and upgrading our solution from Angular 1.6.*",
        },

        {
            "Icon" : "http://www.arduino.org/images/arduino_official_Logo__.png",
            "Name": "Arduino",
            "Description": "I am Investigating Electrical Engineering to compliment Software Development",
        },

        {
            "Icon" : "http://vignette3.wikia.nocookie.net/roblox/images/f/f4/Questionmark.png/revision/latest?cb=20120119031741",
            "Name": "New Technologies",
            "Description": "I am always on the look out for new technologies to explore, by reading via news outlets such as Reddit and MSDN.",
        },
    ];

    // init
    vm.setActiveCompetancy(0);
});
