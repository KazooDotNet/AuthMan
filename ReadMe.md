AuthMan
=======

An extensible, code policy-based authorization manager for DotNet Core.

Install
-------

Install the `AuthMan` and `Microsoft.AspNetCore.Session` nuget packages. Sessions are required to persist state.

Setup
-----

In the standard Kestrel `Startup.cs` file, add something similar to the following code: 
    
    public Startup
    {
        public void ConfigureServices(IServiceCollection services) 
        {
            services
                .AddDistributedMemoryCache();
                .AddSession()
                .Authentication<IAuthMan>(opts => {
                    opts.AddUserAuth<IUserMan, User>();
                });
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
        }
    }

`IAuthMan` can be anything that implements the `IAuthMan` interface. Same thing goes for `IUserMan`. 
`User` in the example above is your user model. You can subclass `AuthMan` (which implements IAuthMan) if you wish to 
add some  convenience methods, or just use it. 

Usage
-----

The middleware (added by `UseAuthentication()` above), puts an instance of `IAuthMan` in the `Items` property
of `HttpContext`. The key is `authMan`. Example:
    
    var authMan = (AuthMan) httpContext.Items["authMan"];

How you get the HttpContext depends on the framework that you're using. For DotNet Core middleware, it is 
passed to the `Invoke` method. 

To see if any authentication methods succeeded, use `authMan.Authenticated()`.

If you need anything finer grained than that, you will need to implement policies. All policies must implement
`IPolicy`. There's a convenience class you can inherit from called `PolicyBase`, which will handle a 
`Before() => bool?` method and the named method passed to `Handle`. Inheriting from `PolicyBase` allows you to do this:

    public class UserPolicy : PolicyBase 
    {
        public bool? Before() 
        {
            if (user.isSuperAdmin) return true; // True or false does not call the requsted method for authorization.
            return null; // Null values will call the requested method for authorization.
        }
        
        public async Task<bool> Read(int userId) 
        {
            // retrieve the record do some logic...
            return canReadUser; // true or false
        }
        
    }
    
    // In some method somehwere
    authMan.Authenticate<UserPolicy>("Read", user.Id); // will throw Exceptions.NotAuthorized if the policy returns false

    // If you don't want it to throw an error, you can use `Can`:
    if (authMan.Can<UserPolicy>("Read", user.Id)) {
        // Do stuff here.
    }
    
You can also set up data scopes depending on the authenticated method if you like. Policies use dependency injection to 
resolve services. 

    public class AccountPolicy : PolicyBase 
    {
        private DbContext _dbContext;
        
        public Account(DbContext context) => _dbContext = context;
        
        public IQueryable<Data.Account> Scope()
        {
          if (SuperAdmin) // Implement your own logic for checking access and current user
            return _dbContext.Accounts;
          return from a in _dbContext.Accounts
            join au in _dbContext.AccountUsers on a.Id equals au.AccountId
            where CurrentUser.Id == au.UserId  
            select a;
        }
    }
    
    // In some method somewhere
    
    var accounts = await authMan.Scope<AccountPolicy>().ToListAsync();
    
You can add arguments to `YourPolicy#Scope()` if you need them. Pass those arguments to `authMan.Scope<>()`.
        

Extending
---------
  
You don't have to use `AddUserAuth`. You can create your own by implenting IAuthenticate and then calling 
`AddAuth<IAuthenticate>()` like so:

    public Startup
    {
        public void ConfigureServices(IServiceCollection services) 
        {
            services.Authentication<IAuthMan>(opts => {
                opts.AddAuth<MyCustomAuthClass>();
            });
        }
    }
    



