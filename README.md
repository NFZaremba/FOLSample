# FOLSampleUsers
FOL Sample Code / C# ASP.NET

Future of Latinos is a community web application that provides the ability for those interested in Latino issues to connect to members of the community, as well as access to legal and educational resources. Members will have resources relating to common issues, such as legal clinics to match an individual’s budget, law school clinics for students studying law, and policy makers who want to contribute content via the Network for Justice.

•	Created Login/Register React page to allow users to safely create and log into accounts. C# used to send and control data flow to SQL database. 

•	Implemented user authorization using React/Redux. Created a component that won’t render its children unless the currently logged in User has the correct role. It is connected to the Redux store where the user role is available to access. Created Redux Store for admin page to be utilized.

•	Leveraged Third Party Logins to give users the ability to login and register using their Facebook, Google, and LinkedIn credentials. C# REST API endpoints were developed for callbacks to store unique token and user registration. 

•	Integrated SendGrid API to send out secure confirmation emails after a user registers. SQL store procedures used to manage user data access and C# to communicate between server and client with WebAPI REST endpoints.
