using System.Linq;
using UnityEngine;
public static class HelperFunctions
{
    /*
    Params: 
    - objectName: string - name of the object calling the function
    - args: list of arguments to check. Can be any object
    */
    /// <summary>
    /// Checks if any of <paramref name="args"/> is null.
    /// </summary>
    /// <param name="objectName">Name of the object that calls <see langword="this"/> function</param>
    /// <param name="args">Any number of parameters to be <see langword="checked"/> <see langword="if"/> they are null </param>
    /// <returns><see langword="bool"/></returns>
    public static bool IsAnyNull(string objectName, params object[] args)
    {
        if (args == null || args.Any(arg => arg == null))
        {
            Debug.Log("["+objectName+"]: object reference is set to null!");
            return true;
        }
        return false;
    }
}