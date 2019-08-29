using System.Threading.Tasks;

using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Platformer.Event {

	public delegate Task SpriteCreationEvent(ICanvasAnimatedControl sender);

}