using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Terraria_Server;
using Terraria_Server.Plugin;
using Terraria_Server.Misc;
using Terraria_Server.Events;
using Terraria_Server.Definitions.Tile;
using System.Net.Sockets;

namespace CropCircles
{
    class CropCircles : Plugin
    {
        bool isEnabled;
        PropertiesFile pfile;

        LinkedList<RegisteredPattern> registeredPatterns;
        LinkedList<Pattern> patterns;


        public override void Load()
        {
            base.Name = "CropCircles";
            base.Description = "Terrain pattern recognition for TDSM.";
            base.Author = "AWRyder";
            base.Version = "0.1";
            base.TDSMBuild = 31;
            this.isEnabled = true;

            Netplay.tcpListener.Server.ReceiveAsync(new SocketAsyncEventArgs() );

            registeredPatterns = new LinkedList<RegisteredPattern>();
            patterns = new LinkedList<Pattern>();

            string ppfolder = Statics.PluginPath + Path.DirectorySeparatorChar + "CropCircles";
            string ppfile = ppfolder + Path.DirectorySeparatorChar + "cropCircles.pfile";

            if (!Directory.Exists(ppfolder))
                 Directory.CreateDirectory(ppfolder);

            if (!File.Exists(ppfile))
                File.Create(ppfile).Close();

            pfile = new PropertiesFile(ppfile);

            int[][] spawnPtrn = new int[3][];
            for (int i = 0; i < spawnPtrn.Length; i++)
            {
                spawnPtrn[i] = new int[2];
            }
            spawnPtrn[0][0] = spawnPtrn[1][0] = spawnPtrn[2][0] = spawnPtrn[0][1] = spawnPtrn[2][1] = 45;
            spawnPtrn[1][1] = -1;


            patterns.AddLast(new Pattern("Spawn Teleport",3, 2, spawnPtrn));

            Program.tConsole.WriteLine(base.Name + " has been enabled.");
        }

        public override void Enable()
        {
            pfile.Load();

            registerHook(Hooks.PLAYER_TILECHANGE);
            registerHook(Hooks.PLAYER_MOVE);

        }

        public override void Disable()
        {
            pfile.Save();
            isEnabled = false;
            Program.tConsole.WriteLine(base.Name + " has been disabled.");
            
        }

        public override void onPlayerMove(PlayerMoveEvent Event)
        {
            Player p = Event.Sender as Player;
            //p.sendMessage("You are on tile: x=" + p.TileLocation.X + " y=" + p.TileLocation.Y);
            foreach ( RegisteredPattern rp in registeredPatterns) {
                //p.sendMessage("Player is in x=" + (int)(Event.Location.X / 16) + " y=" + (int)(Event.Location.Y / 16));
                //p.sendMessage("Rune location is y=" + rp.y + " ( " + (rp.y - 1) + " ) and x between " + rp.x + " and " + rp.x + rp.patt.sizeX);
                if ((int)(Event.Location.Y/16) == (rp.y - rp.patt.sizeY - 1) && ((int)(Event.Location.X/16) >= rp.x && (int)(Event.Location.X/16) <= rp.x+rp.patt.sizeX))
                {
                    p.sendMessage("Activating rune: " + rp.patt.name);
                    p.teleportTo( Main.spawnTileX*16 , Main.spawnTileY*16);
                }
            }
            base.onPlayerMove(Event);
        }

        public override void onPlayerTileChange(PlayerTileChangeEvent Event)
        {
            base.onPlayerTileChange(Event);
            checkRune(Event);
        }

        private void checkRune(PlayerTileChangeEvent Event)
        {
            if (Event.Action == TileAction.BREAK) return;
            int x = (int)Event.Position.X, y = (int)Event.Position.Y;
            for (int p = 0; p < patterns.Count; p++)
            {
                Pattern patt = patterns.ElementAt(p);
                int[][] patArr = patt.pattern;
                
                x = x-patt.sizeX<0?0:x-patt.sizeX;
                y = y-patt.sizeY<0?0:y-patt.sizeY;
                for (int i = 0; i < patt.sizeX * 2; i++)
                {
                    for (int j = 0; j < patt.sizeY * 2; j++)
                    {
                        //For each possible position, lets check for a rune...
                        //int currX = x, currY = y;
                        int currX = x + j, currY = y + i;

                        bool ok = true;
                        for (int zi = 0; zi < patt.sizeX; zi++)
                        {
                            for (int zj = 0; zj < patt.sizeY; zj++)
                            {
                                if (zi == 0 && zj == 0 && Event.Tile.Type == patArr[zi][zj])
                                {
                                    continue;
                                }
                                if (patArr[zi][zj] != -1 && Main.tile.At(currX + zi, currY + zj).Type != patArr[zi][zj])
                                {
                                    //Event.Sender.sendMessage("-> " + Main.tile.At(currX + zj, currY + zi).Type + " != " + patArr[zj][zi] + " and " + zi + "-" + zj + ".");
                                    ok = false;
                                    break;

                                }
                            }
                            if (!ok) break;
                        }
                        if (ok)
                        {
                            registeredPatterns.AddLast(new RegisteredPattern(patt, currX, currY));
                            Event.Sender.sendMessage("Rune "+patt.name+" registered! :D");
                            Event.Sender.sendMessage("in x=" + currX + " y=" + currY);
                            return;
                        }
                        /*else
                        {
                            //Event.Sender.sendMessage("No rune detected! :(");
                        }*/

                        //Main.tile.At(y+j,x+i)
                    }
                }
            }
        }
    }
}
