# Практическая работа 6 — Unity WebXR

**Запуск:** https://gubkagopbop.github.io/webar-practice-6/

Unity WebGL-сцена с кубом на расстоянии 1.2 м перед активной камерой. Куб следует за направлением взгляда — это viewer-space. Для начала AR необходимо действие пользователя и совместимое устройство.

Технологии: Unity 6000.3.9f1, C#, WebXR Export / Interactions 0.25.0 от De-Panther, WebGL и WebAssembly.

## Структура

- `docs` — готовая WebGL-сборка, публикуемая через GitHub Pages.
- `unity-project` — исходный проект Unity. Пакеты WebXR лежат в `Dependencies`, остальные зависимости восстанавливаются через UPM.
- `WebXRTemplate` — адаптированный шаблон WebXR2020.

Сцена: `unity-project/Assets/Scenes/ViewerAR.unity`. Она содержит WebXRManager, WebXRCameraSet, WebXRCamera, TrackedPoseDriver и WebXRController. Используется официальный режим Built-in с отключённым XR Display Subsystem.

Настройки: Compression Format — Disabled, Decompression Fallback — false, Threads Support — false. Размер готовой сборки — 39.29 МиБ без HTTP-сжатия. Отдельный сервер Unity для выполнения не нужен: всё запускается в браузере.

GitHub Pages публикует `/docs` из ветки `main`. Для повторной сборки откройте проект и используйте меню Practice 6 → Build WebGL; оно обновит `docs`. Команда Create scene and configure WebXR пересоздаёт учебную сцену, поэтому предварительно сохраните свои изменения.

На ПК доступен обычный 3D-просмотр. Реальный мобильный AR-тест проводится отдельно; поддержка WebXR определяется устройством и браузером. На Android требуются совместимое устройство и Google Play Services for AR.

WebXR Export: https://github.com/De-Panther/unity-webxr-export. Лицензии Apache-2.0 находятся рядом с пакетами.
