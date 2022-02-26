# Moogle!

![](moogle.png)

> Proyecto de Programación I. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2021.

`Est. David Sardiñas Lima C111`

Moogle! es una aplicación *totalmente original* cuyo propósito es buscar inteligentemente un texto en un conjunto de documentos.

Es una aplicación web, desarrollada con tecnología .NET Core 6.0, específicamente usando Blazor como *framework* web para la interfaz gráfica, y en el lenguaje C#.
La aplicación está dividida en dos componentes fundamentales:

- `MoogleServer` es un servidor web que renderiza la interfaz gráfica y sirve los resultados.
- `MoogleEngine` es una biblioteca de clases donde está implementada la lógica del algoritmo de búsqueda.

### Ejecutando el proyecto

Esta aplicación en cuanto es ejecutada, lee la carpeta `Content` en busca de archivos `*.txt` dentro de esta o en alguna de sus subcarpetas. Después comienza a leer cada uno de los archvios y guardando su contenido. Seguidamente, cada uno de estos contenidos de archivos es procesado, resultando una nuevo  contenido pero sin signos de puntuación u otros símbolos, y fragmentado en palabras. Una vez hecho procedemos a crear un conjunto de todas las palabras que aparecen en el "corpus" (Conjunto de Documentos).
Luego procede a realizar una matriz(m+1 x n) de valores de TF-IDF, del cuál se explicará más adelante; donde el `m` es la cantidad de documentos y `n` es la cantidad de elementos en la Colección de Palabras.

Te preguntarás: ❓ ¿Por qué la dimensión de la matriz es `m+1`?  
Esto se debe a que el (m+1)-ésimo elemento será el valor del campo de busqueda `query`.

Ya realizados estos procesos, la aplicación da el estado de listo ✔ y se puede comenzar a BUSCAR 🔎.

### Aplicación Lista para Buscar
 
Cuando el usuario introduce algo en el campo de búsqueda: `query`, este es procesado por un algoritmo llamado `Levenshtein Distance` (Distancia de Levenshtein) a través del cuál son corregidos los errores de escritura del usuario, o si bien el usuario escribe una palabra correcta pero esta no genera resultados, el algoritmo sugiere una palabra similar que sí genera resultados.

## Evaluación del `score`

De manera general el valor de `score` debe corresponder a cuán relevante es el documento devuelto para la búsqueda realizada. Esto se calcula mediante un `Modelo de Espacio Vectorial`, que se basa en dos pasos fundamentales:
- Primero se representan el contenido de los documentos como vectores de palabras
- Segundo se transforman a un formato numérico para poder realizar actividades de Técnicas Recuperación de Información como extración y filtrado de información.

¿Qué es el TF-IDF?
El `TF-IDF` no es más que un valor numérico que refiere a la relevancia que tiene una palabra en un documento. Este se obtiene de la multiplicación del TF por el IDF (`TF*IDF`).
- El `TF` (Term Frequency) no es más que la frecuencia (repeticiones) de la palabra en el documento.
- El `IDF` (Inverse Document Frequency) es la Frecuencia Inversa de Documento, y se obtiene hallando el logaritmo del cociente entre la cantidad de documentos y la cantidad de repeticiones del la palabra en todos los documentos.

¿Cómo funciona el Modelo de Espacio Vectorial? Breve Resumen.
Vectorización de los documentos: Se le asigna un valor de `TF-IDF` a cada palabra que aparece en el documento.
También se vectoriza el `query` aplicando el mismo procedimiento.
Se calcula la similitud entre el `query` y cada uno de los documentos hallando el valor del coseno del ángulo que existe entre sus respectivos vectores.

## Resultados
Luego en pantalla son mostrados los resultados, apareciendo primero los que mayor `score` posean.
Estos resultados muestran el nombre del Documento y un `snippet` que no es más que una porción del texto dónde aparecen algunos o varios de los términos encontrados.

## Palabras finales

Hasta aquí las ideas de este proyecto, aún se sigue trabajando para su optimización y mejora. 😉😜

