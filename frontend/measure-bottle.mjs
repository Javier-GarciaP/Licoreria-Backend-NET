import { Box3, Vector3 } from 'three';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { pathToFileURL } from 'node:url';

const url = pathToFileURL(new URL('apps/public-web/public/models/vino-tinto/scene.gltf', import.meta.url).pathname).href;
const loader = new GLTFLoader();
loader.load(url, (gltf) => {
  const box = new Box3().setFromObject(gltf.scene, true);
  const size = box.getSize(new Vector3());
  const center = box.getCenter(new Vector3());
  console.log('size', size.x.toFixed(3), size.y.toFixed(3), size.z.toFixed(3));
  console.log('center', center.x.toFixed(3), center.y.toFixed(3), center.z.toFixed(3));
});
