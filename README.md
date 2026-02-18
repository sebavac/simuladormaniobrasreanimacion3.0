# 🚑 Simulador de RCP en Realidad Mixta (Meta Quest 3 + BlazePose)

Un simulador avanzado de Reanimación Cardiopulmonar (RCP) desarrollado en Unity. Combina **Hand Tracking** (seguimiento de manos en Realidad Virtual/Mixta) con **Estimación de Pose por IA (BlazePose)** mediante una webcam externa para crear una experiencia de entrenamiento médica inmersiva y precisa.

## ✨ Características Principales

* **Detección de Paciente por IA:** Utiliza una cámara web de PC para rastrear el cuerpo de un actor o maniquí en el suelo mediante MediaPipe/BlazePose, proyectando un objetivo de compresión anatómica precisa (centro del pecho) en Realidad Mixta.
* **Validación Biométrica de Manos:** Mediante `OVRSkeleton` y `OVRHand`, el sistema exige la técnica correcta de la AHA (Asociación Americana del Corazón):
    * Manos apiladas verticalmente (evita el error de "manos de mariposa").
    * Manos abiertas/dedos entrelazados (rechaza puños cerrados).
    * Palmas orientadas correctamente hacia el pecho del paciente.
* **Feedback de Compresión en Tiempo Real:** Calcula la profundidad exacta de cada compresión. Proporciona feedback visual por colores: Amarillo (Poco profundo), Verde (5-6 cm - Excelente), Rojo (Demasiado profundo).
* **Protocolo de Congelamiento (Freeze):** Control por gestos VR (Pinch del dedo medio) que pausa la lectura de la cámara web. Esto evita que la IA confunda los brazos del rescatista con los del paciente durante la maniobra.

## 🛠️ Requisitos del Sistema

* **Motor:** Unity 2022.3.x (o superior)
* **Hardware VR:** Meta Quest 3 (o Quest 2/Pro compatibles con Hand Tracking)
* **Cámara:** Webcam HD conectada al PC.
* **SDKs Principales:** Meta XR Core SDK, Unity Burst Compiler (para optimización de IA).

## ⚙️ Instalación y Configuración

1.  Clona este repositorio: `git clone https://github.com/TU_USUARIO/TU_REPOSITORIO.git`
2.  Abre el proyecto en Unity Hub.
3.  **Configuración de la Cámara:** Ve a la escena principal, busca el objeto con el script `PCWebcamSource` y asegúrate de configurar el nombre de tu cámara web (o déjalo en blanco para usar la predeterminada).
4.  **Optimización (Burst):** Ve a `Jobs -> Burst -> Enable Compilation` y actívalo para asegurar que la IA procese a 60+ FPS. *(Nota: Si Unity se congela al compilar, borra la carpeta `Library/BurstCache` y reinicia).*

## 📖 Instrucciones de Uso (Modo Práctica Real)

Para utilizar el simulador de forma óptima con otra persona (actor):

1.  **Posicionamiento:** Coloca la cámara web apuntando hacia el suelo en un ángulo de ~45 grados. El actor debe recostarse en el área visible.
2.  **Inicio:** Ejecuta el simulador y ponte el visor. El sistema dirá **"👁️ IA BUSCANDO PACIENTE..."**.
3.  **Calibración y Congelamiento:**
    * Observa cómo el esqueleto virtual se alinea sobre el actor físico.
    * Levanta tu mano izquierda y haz un gesto de **Pellizco con el Dedo Medio y el Pulgar**.
    * El sistema mostrará **"🧊 IA CONGELADA"**. La IA dejará de leer la cámara, fijando el pecho virtual en el espacio físico.
4.  **Ejecución de RCP:** Acércate al paciente, apila tus manos con las palmas abiertas y comienza las compresiones guiándote por la barra de profundidad.

## 📂 Scripts Principales

* `RCPDetector_Master.cs`: El núcleo lógico. Calcula vectores de las manos, valida aperturas de los dedos (`OVRSkeleton`) y mide la distancia hiperprecisa hacia el plano del pecho.
* `PoseSkeletonVisualizer.cs`: Dibuja el cuerpo del paciente basándose en los datos de BlazePose y maneja la lógica de pausa/congelamiento.
* `ControladorCongelamientoVR.cs`: Sistema de control remoto inmersivo que lee el gesto de pinza del usuario para interactuar con la IA sin tocar el PC.

## 🤝 Contribuciones
¡Las contribuciones son bienvenidas! Si tienes ideas para mejorar la física de las compresiones o agregar un metrónomo auditivo, siéntete libre de hacer un *fork* y enviar un *pull request*.
