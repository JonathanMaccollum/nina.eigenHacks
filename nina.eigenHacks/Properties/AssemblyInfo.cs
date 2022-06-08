using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("nina.eigenHacks")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jonathan MacCollum")]
[assembly: AssemblyProduct("nina.eigenHacks")]
[assembly: AssemblyCopyright("Copyright © 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
[assembly: AssemblyMetadata("Tags", "Recoverability,Synchronization")]
// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3ec732eb-72fc-4df8-94fc-00a74b8b9ed6")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.8.1.1002")]
[assembly: AssemblyFileVersion("0.8.1.1002")]
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.0.2001")]
[assembly: AssemblyMetadata("Repository", "https://github.com/JonathanMaccollum/nina.eigenHacks/")]

[assembly: AssemblyMetadata("LongDescription", @"# eigenHacks

## A collection of N.I.N.A. sequencer instructions and triggers for specialized astrophotography use cases.

The following items are currently under early development.

### Recoverability
* Reconnect Camera on Exposure Item Failed:
    * This trigger will allow the main imaging camera to be reconnected in the event an exposure item fails.
    * This trigger is intended as a recovery mechanism for older cameras where sending a disconnect and reconnect resolves image download failures.
    * Note this does not perform any camera warm-up or cool-down actions.
* Center after Exposure Count:
    * This trigger will issue a new Center operation on a target's coordinates after a set number of exposures have been taken.

### Synchronization
* Signal and Wait
    * This instruction is intended for orchestrating multiple imaging cameras on the same mount and pc.  
    * This synchronization primitive allows multiple instances of NINA to reach and wait at a common barrier before proceeding to the next task.
    * When a tag is specified, and the instruction is executed, the instance will wait until other instances of NINA arrive at the same tag.
    * *Careful placement of this instruction and in-person testing is highly recommended.*
* Synchronization Scope
    * This instruction container defines a named scope for where common Signal and Wait instructions can be placed. 
    * When one or more instances of this scope have started executed, the signal and wait instructions will be limited only to instances of nina that are actively running a scope with the same exact name.
    * Signal and Wait instructions that don't exist inside of a Synchronization Scope will share a machine-wide global scope.
! Known Issues: 
    * In the event any one instance of NINA crashes unexpectedly, all instances of NINA must be shut down in order for the Signal and Wait instructions to function correctly.
")]