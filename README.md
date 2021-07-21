# PlayerRecorder 
Records entire round and have option to replay entire round in any time.

Which things are recorded rn?
- Warhead
- Generators
- Doors
- Players
- Items
- Lifts
- Ragdolls
- Scp Termination
- Weapon reload/shot
- SCP914 Knob change

Webhook support for discord:

![Image](https://cdn.discordapp.com/attachments/668651891944587264/867326536154611712/unknown.png)

Custom HUD for viewing replay:

![Image](https://cdn.discordapp.com/attachments/742563439918055510/867385711845703700/unknown.png)

Commands:

(Permission: playerrecorder.end)

- REPLAY end - End replay

(Permission: playerrecorder.pause)

- REPLAY pause - Pause/unpause replay

(Permission: playerrecorder.prepare)

- REPLAY prepare <port> <id> <OPTIONAL: frameId> <OPTIONAL: playerId> - Prepare replay

(Permission: playerrecorder.setspeed)

- REPLAY setspeed <speed> - Set replay speed (default: 0.1)

(Permission: playerrecorder.start)

- REPLAY start - Start replay

Some Info:

- You need to have some seperate server which will be used as replay server.

- Example replay with 55 players round, 20 minutes long can have 20MB ( NOT COMPRESSED ) after compression that can be 7/6MB

- After 5 raw records, everything will be compressed into ZIP.

Patreon:

https://www.patreon.com/playerrecorder
