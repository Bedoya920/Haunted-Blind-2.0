# 🔧 Solucionar Problemas de Instalación de Sentis

## 🚨 **Problemas Comunes con Sentis:**

### **1. Unity Version Compatibility**
Sentis requiere **Unity 2023.3 LTS** o superior:
- **Unity 2023.3 LTS** ✅ (Recomendado)
- **Unity 2024.1** ✅
- **Unity 2024.2** ✅
- **Unity 2023.2** ❌ (No compatible)
- **Unity 2022.x** ❌ (No compatible)

### **2. Package Manager Issues**
- **Conexión a internet** lenta o bloqueada
- **Firewall** bloqueando Unity
- **Proxy** corporativo
- **Cache corrupto** de Unity

## 🛠️ **Soluciones Paso a Paso:**

### **Solución 1: Verificar Versión de Unity**
1. Abre Unity
2. Ve a **Help > About Unity**
3. Verifica la versión:
   - Si es **2023.2 o anterior** → Actualiza Unity
   - Si es **2023.3+** → Continúa con Solución 2

### **Solución 2: Limpiar Cache de Unity**
1. Cierra Unity completamente
2. Ve a la carpeta de cache de Unity:
   - **Windows**: `%LOCALAPPDATA%\Unity\cache\packages\`
   - **Mac**: `~/Library/Unity/cache/packages/`
3. **Elimina** la carpeta `packages`
4. Reinicia Unity

### **Solución 3: Instalación Manual**
1. Ve a **Window > Package Manager**
2. Click en el **+** (Add package)
3. Selecciona **Add package from git URL**
4. Ingresa: `https://github.com/Unity-Technologies/sentis.git?path=/com.unity.sentis`

### **Solución 4: Instalación por Manifest**
1. Ve a `Packages/manifest.json`
2. Agrega esta línea en `dependencies`:
   ```json
   {
     "dependencies": {
       "com.unity.sentis": "2.2.0"
     }
   }
   ```
3. Guarda el archivo
4. Unity debería instalar automáticamente

### **Solución 5: Descargar Manualmente**
1. Ve a: https://github.com/Unity-Technologies/sentis/releases
2. Descarga la versión más reciente
3. Extrae en `Packages/com.unity.sentis/`
4. Reinicia Unity

## 🔍 **Diagnóstico de Problemas:**

### **Error: "Package not found"**
- Verifica tu conexión a internet
- Intenta con VPN si estás en ciertos países
- Verifica que la versión de Unity sea compatible

### **Error: "Installation failed"**
- Cierra Unity completamente
- Elimina la carpeta `Library/PackageCache/`
- Reinicia Unity

### **Error: "Dependencies missing"**
- Instala primero: `com.unity.burst`
- Instala después: `com.unity.sentis`

## 🎯 **Alternativa: Sistema Sin Sentis**

Si no puedes instalar Sentis, el sistema funcionará en **modo fallback**:

### **Características del Modo Fallback:**
- ✅ **Reconocimiento simulado** - Funciona sin modelo real
- ✅ **TTS simulado** - Funciona sin modelo real
- ✅ **IA conversacional** - Funciona completamente
- ✅ **Comandos del juego** - Se ejecutan normalmente
- ✅ **Sistema completo** - Todo funcional

### **Limitaciones del Modo Fallback:**
- ❌ **No reconoce voz real** - Solo simula reconocimiento
- ❌ **No genera audio real** - Solo simula duración
- ❌ **No usa modelos de IA** - Solo respuestas predefinidas

## 🚀 **Setup Sin Sentis:**

Si decides continuar sin Sentis:

1. **El sistema funcionará** en modo simulado
2. **Todas las funcionalidades** estarán disponibles
3. **Puedes probar** el juego completo
4. **Más tarde** puedes instalar Sentis cuando sea posible

## 🎮 **Recomendación:**

### **Para Desarrollo Inmediato:**
- Continúa sin Sentis
- El sistema funcionará perfectamente en modo simulado
- Puedes desarrollar y probar todo el juego

### **Para Producción:**
- Intenta instalar Sentis más tarde
- Usa una versión diferente de Unity
- O usa un sistema alternativo

## 🆘 **Si Nada Funciona:**

### **Opción 1: Usar Unity 2023.3 LTS**
- Descarga Unity Hub
- Instala Unity 2023.3 LTS
- Sentis debería funcionar sin problemas

### **Opción 2: Sistema Híbrido**
- Usar reconocimiento de voz nativo (Windows Speech API)
- Usar TTS nativo (Windows SAPI)
- Mantener IA conversacional

### **Opción 3: Esperar**
- Unity está mejorando Sentis constantemente
- Próximas versiones tendrán mejor compatibilidad

---

## 🎯 **¿Qué Prefieres Hacer?**

1. **Intentar instalar Sentis** con las soluciones anteriores
2. **Continuar sin Sentis** (sistema funcionará en modo simulado)
3. **Usar sistema híbrido** (Windows Speech + IA conversacional)

**¿Cuál opción prefieres?**
