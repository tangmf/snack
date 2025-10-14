# Escape game
## Overall concept
Semi top down 2d game where player is in a very spacious dark store.  
The goal is to escape the store by gathering keys to unlock the door while escaping from monsters lurking in the dark

## Game mechanics
### Player movement
WASD for basic movement. Eight directional movement but animations locked to 4 direction.  
Mouse used for torch movement. Smooth 360 degree movement. Light cone centered on the middle of the cursor.  
Player will face in the direction of the cursor even while moving.

### Light
Torch cannot be turned off. Light cone extending from the player (sourced from the torch) has varying light intensity based on distance from player.  
Intensity strongest close to player, weakest at the tip of the cone.  
Can be blocked by terrain (obstacles may be added if got time).  

### Sound
Base sound level is very quiet. It increases when more keys are obtained by player.  
Sound level produced by player will ramp up to base sound level after continuiously walking for a bit (something like a log curve).   

### Keys
3 keys in level. Each key has a different shape (circle, square, triangle). All 3 necessary to unlock exit door. Keys shown as little icons in UI.  

### Door
Little UI to show which keys are missing when near.

### Darkness Monster
It is kinda attracted to light so it will wonder in a radius near the player (e.g double light cone radius).  
Has a light tolerance value. Decreases when in darkness, increases when in light. When tolerance is surpassed, chase and attack player (instant death on contact).  
Amount added to light tolerance depends on the intensity of the light. Basically, more intense light provokes him faster.  
Try to balance reaction time given to player from 0.5sec to 4sec (No need to stick to numbers here, whatever feels good).  
Will scurry away backwards from light if inside for too long (e.g. 4sec) and light tolerance not surpassed (intention is so it will scurry when being shone with the tip of the light cone).  

### Sound Monster
Blind, can hear in a radius around it (Needs balancing). If it hears something, move closer to it. Wonders randomly in the level if not tracking.  
Speed which it moves increases with the sound level produced (Make it faster than player at max volume).  
Will linger for a little while if it loses track of player (aka player stops moving).  

## Game design
### UI
Basically empty until keys are collected. Keys shown as little icons in UI (shape).  

### Torch
Let it pivot from the player's hand

### Door
Slow flickering green exit sign above door that shows up even when light is not shone on it.  

### Darkness Monster
Crawls on all 4. Unsettling and creepy movement. Gives you the ick like cockroach.

### Sound Monster
Makes distrubing clicking noises when near player.