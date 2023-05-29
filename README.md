# Jump-Bruteforcer
A fun tool for automatically TASing Needle screens. It searches for the fastest input sequence to get from a user-chosen starting point to a user-chosen goal point
and outputs the complete set of inputs, as well as visualizations of the player's trajectory and the parts of the map that were explored. It can also export a macro that can be used with 
[this fork of gm8emulator with macro support](https://github.com/TheBiob/OpenGMK) to create a TAS.
## Usage

https://github.com/namelessiw/Jump-Bruteforcer/assets/33900090/5dec5ee6-e012-4caa-915c-b99553e772f1

This is an example of using the bruteforcer on [Decession](https://delicious-fruit.com/ratings/game_details.php?id=19802). The map is not an exact reproduction of the normal game visuals. 
Instead, it is a transformation of the original visuals, the way it would look to a player shrunk from the normal size of 11X21 pixels down to 1 pixel.
Click 'Import Map' and select a jmap, cmap, or gm82 instances.txt file to start. You can left click to set the start and right click to set the goal.
Or you can type the coordinates into the textboxes.
Then you can click 'Start search' and get up and grab a cup of coffee. Hopefully it will be done when you get back.
You can right click the inputs list to copy it to the clipboard.

The bruteforcer supports solids, playerkillers, water (1, 2, & 3), platforms, and vines. 
Notable unsupported objects include gravity flippers and anything dynamic (jump refreshers, apple animations, etc.).

## Limitations
- Memory and time intensive for large-screens
- The strats will make liberal and sometimes gratuitous use of window-trick, cactusing, and one-frame stutters, and no attempt is made to make them human-friendly
- It is precise up to a 0.2 range of v-aligns, but cannot guarantee finding strats for jumps more precise than that
- The targeted fangame engine is Yuuutu. 
- The mapsize is fixed at 800X608. 
- no A/D support

## How it works
An A* search is performed over the state transition graph, starting from the start node and ending at the goal node, using the distance from the goal as a heuristic.
This distance weights vertical movement less than horizontal movement because jumping/falling is faster than walking.
