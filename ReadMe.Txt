This project can be used as a base project for your new applications.

This project contains three folder BaseProject, BaseProject.BLL && BaseProject.DAL.

In BaseProject Area(i.e Admin Section) is also added that contains default admin panel theme  and navigation.

In Admin Panel we have implamented basic functionality for CMS Manager, Email Template Manager and Paging on User Listing.


There is a BaseController in BaseProject that must be inherited by all the controllers that you will create in your application. Similarly AdminBaseController for all the controllers created in admin panel.

This Base Controller contains some basic modules like authorization, exception handling, log out functionality etc etc.

In this project we have implemented Dependency Injection using Ninject and standard naming convention. To implement that you have to create interfaces and inject that interface in controller's constructor.

For Example: I have injected IErrorLogManager interface in base controller's constructor to log the exceptions.

Please make sure while you implement an interface, implementation should be Explicit because of this, method implementation will not be exposed in Base Project's web layer.
 



