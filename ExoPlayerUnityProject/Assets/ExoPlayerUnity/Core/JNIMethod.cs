using System;
using UnityEngine;

public class JNIMethod
{
    IntPtr classInstance;
    IntPtr id;
    string name;
    string signature;
    jvalue[] parameters;

    public JNIMethod(IntPtr classInstance, string name, string signature)
    {
        this.classInstance = classInstance;
        this.name = name;
        this.signature = signature;
        this.parameters = new jvalue[0]; 
    }

    public void Call(params object[] args)
    {
        if (id == IntPtr.Zero)
        {
            id = AndroidJNI.GetStaticMethodID(classInstance, name, signature);
        }

        if (args != null)
        {
            parameters = AndroidJNIHelper.CreateJNIArgArray(args);
        }

        AndroidJNI.CallStaticVoidMethod(classInstance, id, parameters);
    }
}
