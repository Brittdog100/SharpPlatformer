﻿using System.Threading.Tasks;

using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Platformer.Event {

	public delegate Task SpriteCreationEvent(ICanvasAnimatedControl sender);
	public delegate void CreationEvent();
	public delegate Task TextureCreationEvent(ICanvasAnimatedControl sender);

}