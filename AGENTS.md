# Guidelines for Contributors

- **Port, don't innovate**: Maintain feature parity with the original C# Tilt Brush implementation. Mirror its architecture and logic without introducing new features or redesigns.
- **Three.js target**: All code should run in JavaScript with three.js; remove Unity-specific constructs as you port.
- **Always update the demo**: Whenever functionality changes, update `MinimalExample.html` to visually demonstrate the new or modified feature. Include multiple stroke variations and sufficient lighting so normals can be inspected.
- **Keep strokes verifiable**: Prefer open and closed strokes in examples to expose cross-sections and shape modifiers.
- **Tests are mandatory**: After code changes, run:
  - `npm install`
  - `node -e "import('./js/Pointer.js').then(()=>console.log('ok'))"`
  - `node -e "import('./js/MinimalExample.js').then(()=>console.log('ok'))"`
  - `node js/VerifyTubeBrush.js`
  Make a best effort to ensure these commands succeed.
- **Stub remaining work**: Provide stub implementations for unported C# methods so pending work is explicit.
- **Repository hygiene**: Keep `node_modules` out of version control and leave the working tree clean after commits.

These instructions apply to the entire repository.
