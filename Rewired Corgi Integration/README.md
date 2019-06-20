# Rewired Corgi Integration
## How to use
1. Import the package into your project
2. Resolve namespace conflicts by deleting `InputManager.cs` and `MMInput.cs` from your Corgi import
3. Drop the Rewired Corgi Input Manager into your scene
4. Hit the play button
## Possible Issues
1. "My Jump doesn't work!"
   * Make sure you have Input Manager execution order to -100. You do this from **Edit** > **Project Settings** > **Script Execution Order**
2. "Player 2 can't do anything!"
   * Find the Rewired Integration in your scene and click "Launch Rewired Editor". From there click the "Players" tab and add as many Players as you have. Match the names to your InputManager PlayerIDs
3. "My controller won't work!"
   * Corgi's actions have only been mapped to the keyboard so far, you'll need to create a Joystick Mapping yourself if you want to use a controller.
## Version 1
The first integration strictly ports over Corgi's gameplay input controls into [Rewired](https://assetstore.unity.com/packages/tools/utilities/rewired-21676 "Rewired - Asset Store").
This doesn't fix the need to have an Input Manager per Player. Yet. 
