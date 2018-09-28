using System;
using System.Collections.Generic;

namespace AuthMan
{
  public class AuthManOptions
  {
    
    public List<Type> Authenticators { get; }
    
    public AuthManOptions()
    {
      Authenticators = new List<Type>();
    } 
    
    public AuthManOptions AddUserAuth<TUserMan, TUser>() where TUserMan : IUserMan<TUser>
    {
      Authenticators.Add(typeof(TUserMan));
      return this;
    }

    public AuthManOptions AddAuth<T>() where T : IAuthenticate
    {
      Authenticators.Add(typeof(T));
      return this;
    }
    
  }
}
