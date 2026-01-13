// planet-viewer.js - Three.js Planet Viewer (Lighting Fixed)

class PlanetViewer {
    constructor(containerId) {
        this.containerId = containerId;
        this.container = document.getElementById(containerId);
        this.scene = null;
        this.camera = null;
        this.renderer = null;
        this.planet = null;
        this.atmosphere = null;
        this.clouds = null;
        this.animationId = null;
        this.isInitialized = false;

        // Position du soleil (Lumière directionnelle)
        // X positif = Lumière venant de la droite
        this.sunPosition = new THREE.Vector3(5, 3, 5);
    }

    seededRandom(seed) {
        const x = Math.sin(seed++) * 10000;
        return x - Math.floor(x);
    }

    generateParams(celestialBodyId, planetTypeId) {
        const seed = celestialBodyId * 31 + (planetTypeId || 0);
        let currentSeed = seed;

        const rand = () => {
            currentSeed++;
            return this.seededRandom(currentSeed);
        };

        const varyNear = (base, maxVariation) => {
            const variation = (rand() - 0.5) * 2 * maxVariation;
            return base + variation;
        };

        const templates = {
            1: this.getGasGiantTemplate(rand, varyNear),
            2: this.getNeptuneLikeTemplate(rand, varyNear),
            3: this.getSuperEarthTemplate(rand, varyNear),
            4: this.getTerrestrialTemplate(rand, varyNear),
            5: this.getUnknownTemplate(rand, varyNear)
        };

        return templates[planetTypeId] || templates[4];
    }

    // --- TEMPLATES (Identiques à avant) ---
    getGasGiantTemplate(rand, varyNear) {
        return {
            clouds: null,
            planet: {
                oceanNoise: { scale: varyNear(3.4, 0.3), detail: 5, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.3), distortion: varyNear(0.8, 0.3) },
                earthNoise: { scale: varyNear(3.4, 0.4), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.4), distortion: varyNear(1.2, 0.4) },
                oceanLandRatio: 0.0, snowAmount: 0.0,
                terrainColors: this.generateGasColors(rand),
                oceanColor: null,
                colorSaturation: { min: varyNear(0.6, 0.05), max: varyNear(0.9, 0.05) },
                bumpStrength: varyNear(0.02, 0.01) // Très lisse pour le gaz
            }
        };
    }

    getNeptuneLikeTemplate(rand, varyNear) {
        return {
            clouds: {
                noise: { scale: varyNear(3.4, 0.4), detail: 5, roughness: varyNear(0.75, 0.05), lacunarity: varyNear(2.5, 0.2), distortion: varyNear(0.3, 0.1) },
                cloudAmount: varyNear(0.5, 0.05), cloudDensity: varyNear(1.0, 0.05)
            },
            planet: {
                oceanNoise: { scale: varyNear(3.4, 0.3), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.3), distortion: varyNear(0.6, 0.2) },
                earthNoise: { scale: varyNear(3.4, 0.4), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.3), distortion: varyNear(0.8, 0.2) },
                oceanLandRatio: varyNear(0.8, 0.1), snowAmount: varyNear(0.2, 0.1),
                terrainColors: this.generateIceGiantColors(rand),
                oceanColor: this.rgbToHex(Math.floor(40 + rand() * 20), Math.floor(70 + rand() * 20), Math.floor(110 + rand() * 30)),
                colorSaturation: { min: varyNear(0.7, 0.05), max: varyNear(0.9, 0.05) },
                bumpStrength: varyNear(0.08, 0.03)
            }
        };
    }

    getSuperEarthTemplate(rand, varyNear) {
        return {
            clouds: {
                noise: { scale: varyNear(3.4, 0.3), detail: 5, roughness: varyNear(0.75, 0.05), lacunarity: varyNear(2.5, 0.2), distortion: varyNear(0.3, 0.05) },
                cloudAmount: varyNear(0.5, 0.05), cloudDensity: varyNear(1.0, 0.05)
            },
            planet: {
                oceanNoise: { scale: varyNear(3.4, 0.2), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.2), distortion: varyNear(0.0, 0.05) },
                earthNoise: { scale: varyNear(3.4, 0.3), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.3), distortion: varyNear(0.0, 0.05) },
                oceanLandRatio: varyNear(0.5, 0.1), snowAmount: varyNear(1.0, 0.05),
                terrainColors: this.generateRockyColors(rand),
                oceanColor: this.rgbToHex(Math.floor(0 + rand() * 10), Math.floor(47 + rand() * 10), Math.floor(38 + rand() * 15)),
                colorSaturation: { min: varyNear(0.6, 0.05), max: varyNear(0.85, 0.05) },
                bumpStrength: varyNear(0.3, 0.05)
            }
        };
    }

    getTerrestrialTemplate(rand, varyNear) {
        return {
            clouds: {
                noise: { scale: varyNear(3.4, 0.2), detail: 5, roughness: varyNear(0.75, 0.05), lacunarity: varyNear(2.5, 0.15), distortion: varyNear(0.3, 0.05) },
                cloudAmount: varyNear(0.5, 0.03), cloudDensity: varyNear(1.0, 0.03)
            },
            planet: {
                oceanNoise: { scale: varyNear(3.4, 0.2), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.2), distortion: varyNear(0.0, 0.02) },
                earthNoise: { scale: varyNear(3.4, 0.2), detail: 6, roughness: varyNear(0.65, 0.05), lacunarity: varyNear(3.5, 0.2), distortion: varyNear(0.0, 0.02) },
                oceanLandRatio: varyNear(0.5, 0.05), snowAmount: varyNear(1.0, 0.03),
                terrainColors: this.generateEarthColors(rand),
                oceanColor: this.rgbToHex(Math.floor(0 + rand() * 5), Math.floor(47 + rand() * 8), Math.floor(38 + rand() * 10)),
                colorSaturation: { min: varyNear(0.6, 0.03), max: varyNear(0.85, 0.03) },
                bumpStrength: varyNear(0.2, 0.03)
            }
        };
    }

    getUnknownTemplate(rand, varyNear) {
        const base = this.getTerrestrialTemplate(rand, varyNear);
        if (rand() > 0.5) { base.planet.oceanLandRatio = varyNear(0.5, 0.15); }
        return base;
    }

    // --- COULEURS ---
    generateGasColors(rand) {
        const hueBase = 25 + rand() * 30;
        return [
            this.hslToHex(hueBase, 45 + rand() * 10, 55 + rand() * 10),
            this.hslToHex(hueBase + 5, 40 + rand() * 10, 50 + rand() * 10),
            this.hslToHex(hueBase + 10, 50 + rand() * 10, 60 + rand() * 10),
            this.hslToHex(hueBase - 5, 45 + rand() * 10, 65 + rand() * 10),
            this.hslToHex(hueBase + 15, 55 + rand() * 10, 70 + rand() * 10)
        ];
    }
    generateIceGiantColors(rand) {
        const hueBase = 200 + rand() * 20;
        return [
            this.hslToHex(hueBase, 45 + rand() * 8, 40 + rand() * 8),
            this.hslToHex(hueBase + 5, 50 + rand() * 8, 45 + rand() * 8),
            this.hslToHex(hueBase - 5, 40 + rand() * 8, 35 + rand() * 8),
            this.hslToHex(hueBase + 10, 55 + rand() * 8, 50 + rand() * 8),
            this.hslToHex(hueBase, 60 + rand() * 8, 55 + rand() * 8)
        ];
    }
    generateRockyColors(rand) {
        return [
            this.hslToHex(110 + rand() * 10, 45 + rand() * 8, 35 + rand() * 8),
            this.hslToHex(100 + rand() * 15, 40 + rand() * 8, 45 + rand() * 8),
            this.hslToHex(35 + rand() * 8, 35 + rand() * 8, 40 + rand() * 8),
            this.hslToHex(45 + rand() * 8, 40 + rand() * 8, 50 + rand() * 8),
            this.hslToHex(40 + rand() * 10, 30 + rand() * 8, 60 + rand() * 8)
        ];
    }
    generateEarthColors(rand) {
        const colors = ['#002F26', '#164407', '#6A6A2A', '#539041', '#A4BA8F'];
        if (rand() > 0.7) { colors[1] = this.hslToHex(110 + rand() * 5, 50 + rand() * 5, 25 + rand() * 5); }
        return colors;
    }
    rgbToHex(r, g, b) {
        return '#' + [r, g, b].map(x => {
            const hex = Math.max(0, Math.min(255, Math.round(x))).toString(16);
            return hex.length === 1 ? '0' + hex : hex;
        }).join('');
    }
    hslToHex(h, s, l) {
        l /= 100; const a = s * Math.min(l, 1 - l) / 100;
        const f = n => {
            const k = (n + h / 30) % 12;
            const color = l - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);
            return Math.round(255 * color);
        };
        return this.rgbToHex(f(0), f(8), f(4));
    }
    hexToRgb(hex) {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? { r: parseInt(result[1], 16) / 255, g: parseInt(result[2], 16) / 255, b: parseInt(result[3], 16) / 255 } : { r: 1, g: 1, b: 1 };
    }

    // --- INITIALISATION ---

    async initialize(celestialBodyId, planetTypeId) {
        console.log('🚀 Initializing PlanetViewer...', { celestialBodyId, planetTypeId });

        if (!this.container) return;

        // Scene
        this.scene = new THREE.Scene();

        // Camera
        this.camera = new THREE.PerspectiveCamera(45, this.container.clientWidth / this.container.clientHeight, 0.1, 1000);
        this.camera.position.z = 5;

        // Renderer
        this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
        this.renderer.setSize(this.container.clientWidth, this.container.clientHeight);
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.shadowMap.enabled = true; // Activer les ombres
        this.container.appendChild(this.renderer.domElement);

        // --- ECLAIRAGE DRAMATIQUE ---
        // 1. Soleil (Fort et directionnel)
        const sun = new THREE.DirectionalLight(0xffffff, 2.0);
        sun.position.copy(this.sunPosition);
        this.scene.add(sun);

        // 2. Lumière ambiante FAIBLE (Pour que le côté nuit soit sombre mais pas invisible)
        // Mettre à 0.05 ou 0.1 pour un effet nuit noire
        const ambient = new THREE.AmbientLight(0xffffff, 0.05);
        this.scene.add(ambient);

        // 3. Backlight subtil (bleuté) pour détacher la planète du fond
        const rimLight = new THREE.DirectionalLight(0x404060, 0.5);
        rimLight.position.set(-5, 0, -5);
        this.scene.add(rimLight);

        const params = this.generateParams(celestialBodyId, planetTypeId);
        const loader = new THREE.GLTFLoader();

        try {
            console.log('📦 Loading GLB...');
            const gltf = await new Promise((resolve, reject) => {
                loader.load('/assets/models/GeneratedPlanet.glb', resolve, undefined, reject);
            });

            const root = gltf.scene;

            // Parcours pour trouver les objets
            root.traverse((child) => {
                if (child.isMesh) {
                    const name = child.name.toLowerCase();
                    child.frustumCulled = false;

                    if (name.includes('atmosphere') || name.includes('icosphere.002')) {
                        this.atmosphere = child;
                        this.createAtmosphereShader(child, params);
                    }
                    else if (name.includes('cloud') || name.includes('icosphere.001')) {
                        this.clouds = child;
                        if (params.hasClouds && params.clouds) {
                            this.applyCloudShader(child, params.clouds);
                        } else {
                            child.visible = false;
                        }
                    }
                    else if (name.includes('planet') || name.includes('icosphere')) {
                        this.planet = child;
                        this.applyPlanetShader(child, params.planet);
                        child.material.side = THREE.FrontSide;
                    }
                }
            });

            // Normalisation / Fallback
            if (this.planet) {
                this.planet.geometry.computeBoundingBox();
                const box = this.planet.geometry.boundingBox;
                const size = new THREE.Vector3();
                box.getSize(size);
                const maxDim = Math.max(size.x, size.y, size.z);

                if (maxDim <= 0.001 || !isFinite(maxDim)) {
                    console.warn("⚠️ Invalid Geometry (Size 0). Replacing with generated Sphere.");
                    // Fallback Géométrie
                    this.planet.geometry.dispose();
                    this.planet.geometry = new THREE.SphereGeometry(1.5, 128, 128); // Haute résol
                    root.position.set(0, 0, 0);
                    root.scale.set(1, 1, 1);
                    root.rotation.set(0, 0, 0);
                } else {
                    const center = new THREE.Vector3();
                    box.getCenter(center);
                    root.position.copy(center).negate();
                    const scale = 3.0 / maxDim;
                    root.scale.set(scale, scale, scale);
                }
            }

            this.scene.add(root);
            this.isInitialized = true;
            this.container.classList.add('loaded');
            this.animate();

        } catch (error) {
            console.error('❌ Error:', error);
            this.createFallbackSphere(params);
            this.container.classList.add('loaded');
        }

        window.addEventListener('resize', () => this.onWindowResize());
    }

    createFallbackSphere(params) {
        const geo = new THREE.SphereGeometry(1.5, 64, 64);
        const mesh = new THREE.Mesh(geo);
        this.applyPlanetShader(mesh, params.planet);
        this.scene.add(mesh);
        this.planet = mesh;
        this.isInitialized = true;
        this.animate();
    }

    createAtmosphereShader(mesh, params) {
        // Atmosphère : on ajoute une "Sun Direction" pour que le glow soit plus fort côté soleil
        mesh.material = new THREE.ShaderMaterial({
            vertexShader: `
                varying vec3 vNormal; 
                varying vec3 vPosition;
                void main() { 
                    vNormal = normalize(normalMatrix * normal); 
                    vPosition = (modelViewMatrix * vec4(position, 1.0)).xyz;
                    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0); 
                }
            `,
            fragmentShader: `
                varying vec3 vNormal; 
                varying vec3 vPosition;
                uniform vec3 color; 
                uniform vec3 sunPosition;
                void main() { 
                    // Fresnel (Bord de la planète)
                    float viewAngle = dot(vNormal, vec3(0, 0, 1));
                    float fresnel = pow(0.6 - viewAngle, 4.0);
                    
                    // Lumière du soleil (pour que l'atmosphère brille du bon côté)
                    vec3 sunDir = normalize(sunPosition); 
                    float sunIntensity = max(dot(vNormal, sunDir), 0.0);
                    
                    // Combine
                    float alpha = fresnel * (sunIntensity * 0.8 + 0.2);
                    gl_FragColor = vec4(color, alpha); 
                }
            `,
            uniforms: {
                color: { value: new THREE.Color(params.atmosphereColor) },
                sunPosition: { value: this.sunPosition }
            },
            blending: THREE.AdditiveBlending, side: THREE.BackSide, transparent: true, depthWrite: false
        });
    }

    applyPlanetShader(mesh, params) {
        const uniforms = {
            // ... (Bruit et Couleurs inchangés) ...
            oceanNoiseScale: { value: params.oceanNoise.scale },
            oceanNoiseDetail: { value: params.oceanNoise.detail },
            oceanNoiseRoughness: { value: params.oceanNoise.roughness },
            oceanNoiseLacunarity: { value: params.oceanNoise.lacunarity },
            oceanNoiseDistortion: { value: params.oceanNoise.distortion },
            earthNoiseScale: { value: params.earthNoise.scale },
            earthNoiseDetail: { value: params.earthNoise.detail },
            earthNoiseRoughness: { value: params.earthNoise.roughness },
            earthNoiseLacunarity: { value: params.earthNoise.lacunarity },
            earthNoiseDistortion: { value: params.earthNoise.distortion },
            oceanLandRatio: { value: params.oceanLandRatio },
            snowAmount: { value: params.snowAmount },
            terrainColor0: { value: new THREE.Color(params.terrainColors[0]) },
            terrainColor1: { value: new THREE.Color(params.terrainColors[1]) },
            terrainColor2: { value: new THREE.Color(params.terrainColors[2]) },
            terrainColor3: { value: new THREE.Color(params.terrainColors[3]) },
            terrainColor4: { value: new THREE.Color(params.terrainColors[4]) },
            oceanColor: { value: params.oceanColor ? new THREE.Color(params.oceanColor) : new THREE.Color(0x002F26) },
            saturationMin: { value: params.colorSaturation.min },
            saturationMax: { value: params.colorSaturation.max },
            bumpStrength: { value: params.bumpStrength },

            // NOUVEAU : Position du soleil passée au shader
            sunPosition: { value: this.sunPosition }
        };

        const vertexShader = `
            varying vec3 vNormal;
            varying vec3 vPosition;
            varying vec2 vUv;
            void main() {
                vNormal = normalize(normalMatrix * normal);
                vPosition = position;
                vUv = uv;
                gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
            }
        `;

        const fragmentShader = `
            uniform float oceanNoiseScale;
            uniform float oceanNoiseDetail;
            uniform float oceanNoiseRoughness;
            uniform float oceanNoiseLacunarity;
            uniform float oceanNoiseDistortion;
            uniform float earthNoiseScale;
            uniform float earthNoiseDetail;
            uniform float earthNoiseRoughness;
            uniform float earthNoiseLacunarity;
            uniform float earthNoiseDistortion;
            uniform float oceanLandRatio;
            uniform float snowAmount;
            uniform vec3 terrainColor0;
            uniform vec3 terrainColor1;
            uniform vec3 terrainColor2;
            uniform vec3 terrainColor3;
            uniform vec3 terrainColor4;
            uniform vec3 oceanColor;
            uniform float saturationMin;
            uniform float saturationMax;
            uniform float bumpStrength;
            
            // Lumière
            uniform vec3 sunPosition;

            varying vec3 vNormal;
            varying vec3 vPosition;
            varying vec2 vUv;
            
            // --- NOISE FUNCTIONS (Copier le bloc 'mod289' jusqu'à 'fbm' d'avant) ---
            vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 permute(vec4 x) { return mod289(((x*34.0)+1.0)*x); }
            vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            float snoise(vec3 v) {
                const vec2 C = vec2(1.0/6.0, 1.0/3.0);
                const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);
                vec3 i = floor(v + dot(v, C.yyy));
                vec3 x0 = v - i + dot(i, C.xxx);
                vec3 g = step(x0.yzx, x0.xyz);
                vec3 l = 1.0 - g;
                vec3 i1 = min(g.xyz, l.zxy);
                vec3 i2 = max(g.xyz, l.zxy);
                vec3 x1 = x0 - i1 + C.xxx;
                vec3 x2 = x0 - i2 + C.yyy;
                vec3 x3 = x0 - D.yyy;
                i = mod289(i);
                vec4 p = permute(permute(permute(i.z + vec4(0.0, i1.z, i2.z, 1.0)) + i.y + vec4(0.0, i1.y, i2.y, 1.0)) + i.x + vec4(0.0, i1.x, i2.x, 1.0));
                float n_ = 0.142857142857;
                vec3 ns = n_ * D.wyz - D.xzx;
                vec4 j = p - 49.0 * floor(p * ns.z * ns.z);
                vec4 x_ = floor(j * ns.z);
                vec4 y_ = floor(j - 7.0 * x_);
                vec4 x = x_ * ns.x + ns.yyyy;
                vec4 y = y_ * ns.x + ns.yyyy;
                vec4 h = 1.0 - abs(x) - abs(y);
                vec4 b0 = vec4(x.xy, y.xy);
                vec4 b1 = vec4(x.zw, y.zw);
                vec4 s0 = floor(b0) * 2.0 + 1.0;
                vec4 s1 = floor(b1) * 2.0 + 1.0;
                vec4 sh = -step(h, vec4(0.0));
                vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
                vec3 p0 = vec3(a0.xy, h.x);
                vec3 p1 = vec3(a0.zw, h.y);
                vec3 p2 = vec3(a1.xy, h.z);
                vec3 p3 = vec3(a1.zw, h.w);
                vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
                p0 *= norm.x; p1 *= norm.y; p2 *= norm.z; p3 *= norm.w;
                vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                m = m * m;
                return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
            }
            float fbm(vec3 p, float scale, int octaves, float roughness, float lacunarity) {
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = scale;
                for(int i = 0; i < 8; i++) {
                    if(i >= octaves) break;
                    value += amplitude * snoise(p * frequency);
                    frequency *= lacunarity;
                    amplitude *= roughness;
                }
                return value;
            }

            void main() {
                vec3 pos = normalize(vPosition);
                
                // --- GÉNÉRATION TERRAIN (Inchangé) ---
                float oceanNoise = fbm(pos, oceanNoiseScale, int(oceanNoiseDetail), oceanNoiseRoughness, oceanNoiseLacunarity);
                oceanNoise = oceanNoise * 0.5 + 0.5;
                
                vec3 distortedPos = pos + oceanNoise * oceanNoiseDistortion * 0.1;
                float earthNoise = fbm(distortedPos, earthNoiseScale, int(earthNoiseDetail), earthNoiseRoughness, earthNoiseLacunarity);
                earthNoise = earthNoise * 0.5 + 0.5;
                
                float isOcean = step(oceanNoise, oceanLandRatio);
                
                vec3 finalColor;
                if(isOcean > 0.5) {
                    finalColor = oceanColor;
                } else {
                    float t = earthNoise;
                    if(t < 0.25) finalColor = mix(terrainColor0, terrainColor1, t * 4.0);
                    else if(t < 0.5) finalColor = mix(terrainColor1, terrainColor2, (t - 0.25) * 4.0);
                    else if(t < 0.75) finalColor = mix(terrainColor2, terrainColor3, (t - 0.5) * 4.0);
                    else finalColor = mix(terrainColor3, terrainColor4, (t - 0.75) * 4.0);
                }
                
                float snowFactor = smoothstep(snowAmount, 1.0, earthNoise);
                finalColor = mix(finalColor, vec3(1.0), snowFactor * (1.0 - isOcean));
                
                float gray = dot(finalColor, vec3(0.299, 0.587, 0.114));
                float saturation = mix(saturationMin, saturationMax, earthNoise);
                finalColor = mix(vec3(gray), finalColor, saturation);

                // --- ECLAIRAGE CORRIGÉ ---
                vec3 sunDir = normalize(sunPosition);
                // Calcul de l'intensité de la lumière (Dot product)
                float lightIntensity = dot(vNormal, sunDir);
                
                // Rendre l'ombre vraiment sombre (ex: 0.05 au lieu de 0.3)
                // "max" empêche les valeurs négatives, mais on garde un fond très faible
                float dayNightFactor = max(lightIntensity, 0.05);

                // Appliquer l'ombre sur la couleur
                finalColor *= dayNightFactor;

                gl_FragColor = vec4(finalColor, 1.0);
            }
        `;

        mesh.material = new THREE.ShaderMaterial({
            uniforms: uniforms,
            vertexShader: vertexShader,
            fragmentShader: fragmentShader,
            side: THREE.FrontSide
        });
    }

    applyCloudShader(mesh, params) {
        console.log('☁️ Applying cloud shader...');

        const uniforms = {
            // ... (Params inchangés) ...
            cloudNoiseScale: { value: params.noise.scale },
            cloudNoiseDetail: { value: params.noise.detail },
            cloudNoiseRoughness: { value: params.noise.roughness },
            cloudNoiseLacunarity: { value: params.noise.lacunarity },
            cloudNoiseDistortion: { value: params.noise.distortion },
            cloudAmount: { value: params.cloudAmount },
            cloudDensity: { value: params.cloudDensity },
            time: { value: 0 },
            // NOUVEAU : Position du soleil
            sunPosition: { value: this.sunPosition }
        };

        const vertexShader = `
            varying vec3 vPosition;
            varying vec3 vNormal;
            void main() {
                vPosition = position;
                vNormal = normalize(normalMatrix * normal);
                gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
            }
        `;

        const fragmentShader = `
            uniform float cloudNoiseScale;
            uniform float cloudNoiseDetail;
            uniform float cloudNoiseRoughness;
            uniform float cloudNoiseLacunarity;
            uniform float cloudNoiseDistortion;
            uniform float cloudAmount;
            uniform float cloudDensity;
            uniform float time;
            
            // Lumière
            uniform vec3 sunPosition;

            varying vec3 vPosition;
            varying vec3 vNormal;
            
            // NOISE FUNCTIONS (Copier le bloc ici aussi)
            vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            vec4 permute(vec4 x) { return mod289(((x*34.0)+1.0)*x); }
            vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
            float snoise(vec3 v) {
                const vec2 C = vec2(1.0/6.0, 1.0/3.0);
                const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);
                vec3 i = floor(v + dot(v, C.yyy));
                vec3 x0 = v - i + dot(i, C.xxx);
                vec3 g = step(x0.yzx, x0.xyz);
                vec3 l = 1.0 - g;
                vec3 i1 = min(g.xyz, l.zxy);
                vec3 i2 = max(g.xyz, l.zxy);
                vec3 x1 = x0 - i1 + C.xxx;
                vec3 x2 = x0 - i2 + C.yyy;
                vec3 x3 = x0 - D.yyy;
                i = mod289(i);
                vec4 p = permute(permute(permute(i.z + vec4(0.0, i1.z, i2.z, 1.0)) + i.y + vec4(0.0, i1.y, i2.y, 1.0)) + i.x + vec4(0.0, i1.x, i2.x, 1.0));
                float n_ = 0.142857142857;
                vec3 ns = n_ * D.wyz - D.xzx;
                vec4 j = p - 49.0 * floor(p * ns.z * ns.z);
                vec4 x_ = floor(j * ns.z);
                vec4 y_ = floor(j - 7.0 * x_);
                vec4 x = x_ * ns.x + ns.yyyy;
                vec4 y = y_ * ns.x + ns.yyyy;
                vec4 h = 1.0 - abs(x) - abs(y);
                vec4 b0 = vec4(x.xy, y.xy);
                vec4 b1 = vec4(x.zw, y.zw);
                vec4 s0 = floor(b0) * 2.0 + 1.0;
                vec4 s1 = floor(b1) * 2.0 + 1.0;
                vec4 sh = -step(h, vec4(0.0));
                vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
                vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
                vec3 p0 = vec3(a0.xy, h.x);
                vec3 p1 = vec3(a0.zw, h.y);
                vec3 p2 = vec3(a1.xy, h.z);
                vec3 p3 = vec3(a1.zw, h.w);
                vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
                p0 *= norm.x; p1 *= norm.y; p2 *= norm.z; p3 *= norm.w;
                vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
                m = m * m;
                return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
            }
            float fbm(vec3 p, float scale, int octaves, float roughness, float lacunarity) {
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = scale;
                for(int i = 0; i < 8; i++) {
                    if(i >= octaves) break;
                    value += amplitude * snoise(p * frequency);
                    frequency *= lacunarity;
                    amplitude *= roughness;
                }
                return value;
            }

            void main() {
                vec3 pos = normalize(vPosition);
                float cloudNoise = fbm(pos, cloudNoiseScale, int(cloudNoiseDetail), cloudNoiseRoughness, cloudNoiseLacunarity);
                cloudNoise = cloudNoise * 0.5 + 0.5;
                float clouds = smoothstep(cloudAmount, 1.0, cloudNoise);
                clouds *= cloudDensity;
                
                // Transparence
                float alpha = clouds;
                if (alpha < 0.01) discard;

                // ECLAIRAGE NUAGES
                vec3 sunDir = normalize(sunPosition);
                float lightIntensity = max(dot(vNormal, sunDir), 0.05);
                
                // Couleur nuage blanche tamisée par la lumière
                vec3 finalCloudColor = vec3(1.0) * lightIntensity;
                
                gl_FragColor = vec4(finalCloudColor, alpha);
            }
        `;

        mesh.material = new THREE.ShaderMaterial({
            uniforms: uniforms,
            vertexShader: vertexShader,
            fragmentShader: fragmentShader,
            transparent: true,
            side: THREE.DoubleSide,
            depthWrite: false
        });
    }

    animate() {
        if (!this.isInitialized) return;
        this.animationId = requestAnimationFrame(() => this.animate());

        // Rotation de la planète
        if (this.planet) this.planet.rotation.y += 0.002;
        if (this.clouds) this.clouds.rotation.y += 0.0025;
        if (this.atmosphere) this.atmosphere.rotation.y += 0.002;

        this.renderer.render(this.scene, this.camera);
    }

    // ... onWindowResize et dispose inchangés
    onWindowResize() {
        if (!this.container || !this.camera || !this.renderer) return;
        this.camera.aspect = this.container.clientWidth / this.container.clientHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(this.container.clientWidth, this.container.clientHeight);
    }
    dispose() {
        if (this.animationId) cancelAnimationFrame(this.animationId);
        if (this.renderer) {
            this.renderer.dispose();
            if (this.renderer.domElement && this.renderer.domElement.parentNode) this.renderer.domElement.parentNode.removeChild(this.renderer.domElement);
        }
        this.isInitialized = false;
    }
}
// ... Export inchangé
export function createViewer(containerId, celestialBodyId, planetTypeId) {
    if (typeof THREE === 'undefined' || typeof THREE.GLTFLoader === 'undefined') return;
    const viewer = new PlanetViewer(containerId);
    viewer.initialize(celestialBodyId, planetTypeId);
    return { dispose: () => viewer.dispose() };
}
window.PlanetViewer = PlanetViewer;