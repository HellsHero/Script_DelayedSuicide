$Server::delaySuicide = 1; //Enable/disable script
$Server::delaySuicide::time = 10; //In seconds
package script_delaySuicide
{
    function serverCmdSuicide(%client)
    {
        if(isObject(%pl = %client.player))
            if(isObject(%client.minigame) && $Server::delaySuicide && !%pl.canSuicide)
                %pl.suicideSequence(0);
            else
                parent::serverCmdSuicide(%client);
    }
    
    function Armor::damage(%this, %obj, %sourceObject, %position, %damage, %damageType)
	{
	    if(isObject(%obj) && isObject(%obj.client.minigame) && $Server::delaySuicide)
	        %pl.suicideSequence(1);
		Parent::damage(%this, %obj, %sourceObject, %position, %damage, %damageType);
	}
};
activatePackage(script_delaySuicide);

function Player::suicideSequence(%this,%cancel)
{
    if(isEventPending(%pl.suicideDelay))
        cancel(%pl.suicideDelay);
    
    %client = %pl.client;
    if(%cancel)
    {
        messageClient(%client,'',"\c6Suicide canceled! ( " @ %pl.suicideSequence @ "/" @ $Server::delaySuicide::time @ " )");
        %pl.suicidePos = "";
        %pl.suicideSequence = "";
        return;
    }
    
    if(%pl.suicideSequence $= "" || %pl.suicideSequence == 0)
    {
        echo("Suicide Sequence, initial go through" SPC %pl.suicideSequence SPC ":" SPC %pl.suicidePos);
        %pl.suicideSequence = 0;
        %pl.suicidePos = %pl.getPosition();
    }
    
    if(vectorDist(%pl.getPosition(),%pl.suicidePos) > 0.5)
    {
        messageClient(%client,'',"\c6Suicide canceled! ( " @ %pl.suicideSequence @ "/" @ $Server::delaySuicide::time @ " )");
        %pl.suicidePos = "";
        %pl.suicideSequence = "";
        return;
    }
    
    if(%pl.suicideSequence == $Server::delaySuicide::time)
    {
        %pl.suicideSequence = "";
        %pl.suicidePos = "";
        %pl.canSuicide = 1;
        call(serverCmdSuicide(%client));
        return;
    }
    %timeLeft = $Server::delaySuicide::time-%pl.suicideSequence;
    messageClient(%client,'',"\c6Suicide in " @ %timeleft SPC ((%timeLeft == 1) ? "second" : "seconds"));
    %pl.suicideSequence++;
    
    %pl.suicideDelay = %pl.schedule(1000,suicideSequence,%cancel);
}

function serverCmdDelaySuicide(%client,%a)
{
    if(!%client.isAdmin)
        return;
    if(%a $= "" && %b $= "")
    {
        messageClient(%client,'',"\c6Type \c3/delaySuicide A");
        messageClient(%client,'',"\c3A \c6can be replaced with \c3toggle\c6 or a \c3number\c6 which represents seconds");
    }
    else if(%a $= "toggle")
        messageClient(%client,'',"\c6DelaySuicide script is now\c3 " @ (($Server::delaySuicide = !$Server::delaySuicide) ? "enabled" : "disabled"));
    else if(%a > 0)
        messageClient(%client,'',"\c6DelaySuicide time is now\c3 " @ ($Server::delaySuicide::time = %a));
}