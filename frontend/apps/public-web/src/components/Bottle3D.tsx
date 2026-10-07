import { Suspense, useRef } from 'react';
import { Canvas, useFrame } from '@react-three/fiber';
import { Center, Environment, Lightformer, useGLTF } from '@react-three/drei';
import type { Group } from 'three';

const MODEL_URL = '/models/vino-tinto/scene.gltf';

function Bottle() {
  const { scene } = useGLTF(MODEL_URL);
  const group = useRef<Group>(null);

  useFrame((_, delta) => {
    if (group.current) group.current.rotation.y += delta * 0.32;
  });

  return (
    <group ref={group}>
      <Center>
        <primitive object={scene} />
      </Center>
    </group>
  );
}

interface Bottle3DProps {
  className?: string;
}

/**
 * Vitrina de producto: la botella de vino tinto "Castaño Colección" flota
 * cruda sobre el óxido, sin tarjeta ni marco. Iluminación cálida de barra.
 */
export function Bottle3D({ className }: Bottle3DProps) {
  return (
    <div className={className} aria-hidden="true">
      <Canvas
        dpr={[1, 2]}
        camera={{ position: [0, 0.6, 40], fov: 32 }}
        gl={{ alpha: true, antialias: true }}
        style={{ background: 'transparent' }}
      >
        <ambientLight intensity={0.4} color="#ffd9a0" />
        <directionalLight position={[6, 12, 8]} intensity={2.4} color="#ffcf7a" />
        <pointLight position={[10, 6, -8]} intensity={70} color="#faae33" distance={60} />
        <pointLight position={[-9, -2, 7]} intensity={45} color="#d1255c" distance={55} />

        <Suspense fallback={null}>
          <Bottle />
          <Environment resolution={256}>
            <Lightformer intensity={3} color="#ffcf7a" position={[0, 6, -6]} scale={[12, 8, 1]} />
            <Lightformer intensity={1.4} color="#d1255c" position={[-6, 0, 4]} scale={[6, 12, 1]} />
            <Lightformer intensity={2.2} color="#faae33" position={[6, 2, 4]} scale={[6, 12, 1]} />
          </Environment>
        </Suspense>
      </Canvas>
    </div>
  );
}

useGLTF.preload(MODEL_URL);
