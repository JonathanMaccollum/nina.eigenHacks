# eigenHacks

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
    * When one instance of Nina closes gracefully while other instances are waiting for it to signal, those other instances will no longer have to wait and will start to execute the next instruction instantly.
* Synchronization Scope
    * This instruction container defines a named scope for where common Signal and Wait instructions can be placed. 
    * When one or more instances of this scope have started executed, the signal and wait instructions will be limited only to instances of nina that are actively running a scope with the same exact name.
    * When one instance of NINA has a scope active but then completes while another instance is waiting for a signal from the first one, the other instance will no longer have to wait and will begin to execute the next instruction in the sequence.
    * Signal and Wait instructions that don't exist inside of a Synchronization Scope will share a machine-wide global scope.
! Known Issues: 
    * In the event any one instance of NINA crashes unexpectedly, all instances of NINA must be shut down in order for the Signal and Wait instructions to function correctly.