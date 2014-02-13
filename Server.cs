$Server::delayedSuicide = 1; //Enable/disable script
$Server::delayedSuicide::time = 10; //In seconds
package script_delayedSuicide
{
    function serverCmdSuicide(%client)
    {
        %pl = %client.player;
        if(isObject(%pl))
            if(isObject(%client.minigame) && $Server::delayedSuicide && !%pl.canSuicide)
                %pl.suicideSequence(0);
            else
                parent::serverCmdSuicide(%client);
    }
    
    function Armor::damage(%this, %obj, %sourceObject, %position, %damage, %damageType)
	{
	    if(isObject(%obj) && isObject(%obj.client.minigame) && $Server::delayedSuicide)
	        if(isEventPending(%obj.suicideDelay))
	            %obj.suicideSequence(1);
		Parent::damage(%this, %obj, %sourceObject, %position, %damage, %damageType);
	}
};
activatePackage(script_delayedSuicide);

function Player::suicideSequence(%this,%cancel)
{
    if(isEventPending(%this.suicideDelay))
        cancel(%this.suicideDelay);
    
    %client = %this.client;
    if(%cancel)
    {
        messageClient(%client,'',"\c6Suicide canceled!");
        %this.suicidePos = "";
        %this.suicideSequence = "";
        return;
    }
    
    if(%this.suicideSequence $= "" || %this.suicideSequence == 0)
    {
        echo("Suicide Sequence, initial go through" SPC %this.suicideSequence SPC ":" SPC %this.suicidePos);
        %this.suicideSequence = 0;
        %this.suicidePos = %this.getPosition();
    }
    
    if(vectorDist(%this.getPosition(),%this.suicidePos) != 0)
    {
        messageClient(%client,'',"\c6Suicide canceled!");
        %this.suicidePos = "";
        %this.suicideSequence = "";
        return;
    }
    
    if(%this.suicideSequence == $Server::delayedSuicide::time)
    {
        %this.suicideSequence = "";
        %this.suicidePos = "";
        %this.canSuicide = 1;
        call(serverCmdSuicide(%client));
        return;
    }
    %timeLeft = $Server::delayedSuicide::time-%this.suicideSequence;
    messageClient(%client,'',"\c6Suicide in " @ %timeleft SPC ((%timeLeft == 1) ? "second" : "seconds"));
    %this.suicideSequence++;
    
    %this.suicideDelay = %this.schedule(1000,suicideSequence,%cancel);
}

function serverCmdDelayedSuicide(%client,%a)
{
    if(!%client.isAdmin)
        return;
    if(%a $= "" && %b $= "")
    {
        messageClient(%client,'',"\c6Type \c3/delayedSuicide A");
        messageClient(%client,'',"\c3A \c6can be replaced with \c3toggle\c6 or a \c3number\c6 which represents seconds");
    }
    else if(%a $= "toggle")
        messageClient(%client,'',"\c6DelayedSuicide script is now\c3 " @ (($Server::delayedSuicide = !$Server::delayedSuicide) ? "enabled" : "disabled"));
    else if(%a > 0)
        messageClient(%client,'',"\c6DelayedSuicide time is now\c3 " @ ($Server::delayedSuicide::time = %a));
}