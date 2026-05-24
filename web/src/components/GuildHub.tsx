import { useEffect, useRef, useState, type MouseEvent, type PointerEvent } from "react";
import { guildAssets, hotspots } from "../data/demoData";
import type { HotspotDefinition, HotspotId, Language } from "../types";
import styles from "../App.module.css";

interface MaskState {
  definition: HotspotDefinition;
  image: HTMLImageElement;
  canvas: HTMLCanvasElement;
  context: CanvasRenderingContext2D;
}

interface GuildHubProps {
  language: Language;
  translate: (key: string) => string;
  debugHotspots: boolean;
  onOpenHotspot: (hotspot: HotspotId) => void;
}

export default function GuildHub({ language, translate, debugHotspots, onOpenHotspot }: GuildHubProps) {
  const rootRef = useRef<HTMLDivElement>(null);
  const masksRef = useRef<MaskState[]>([]);
  const [activeHotspot, setActiveHotspot] = useState<HotspotId | null>(null);

  useEffect(() => {
    let cancelled = false;

    Promise.all(
      hotspots.map(
        (definition) =>
          new Promise<MaskState>((resolve, reject) => {
            const image = new Image();
            image.onload = () => {
              const canvas = document.createElement("canvas");
              canvas.width = image.naturalWidth;
              canvas.height = image.naturalHeight;
              const context = canvas.getContext("2d", { willReadFrequently: true });
              if (!context) {
                reject(new Error(`Cannot create canvas for ${definition.id}`));
                return;
              }

              context.drawImage(image, 0, 0);
              resolve({ definition, image, canvas, context });
            };
            image.onerror = () => reject(new Error(`Cannot load hotspot mask ${definition.maskSrc}`));
            image.src = definition.maskSrc;
          })
      )
    )
      .then((loadedMasks) => {
        if (!cancelled) {
          masksRef.current = loadedMasks;
        }
      })
      .catch((error) => {
        console.error(error);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  function findHotspotAt(clientX: number, clientY: number) {
    const rect = rootRef.current?.getBoundingClientRect();
    const firstMask = masksRef.current[0];
    if (!rect || !firstMask) {
      return null;
    }

    const scale = Math.max(rect.width / firstMask.image.naturalWidth, rect.height / firstMask.image.naturalHeight);
    const displayedWidth = firstMask.image.naturalWidth * scale;
    const displayedHeight = firstMask.image.naturalHeight * scale;
    const offsetX = (rect.width - displayedWidth) / 2;
    const offsetY = (rect.height - displayedHeight) / 2;
    const imageX = Math.floor((clientX - rect.left - offsetX) / scale);
    const imageY = Math.floor((clientY - rect.top - offsetY) / scale);

    if (
      imageX < 0 ||
      imageY < 0 ||
      imageX >= firstMask.image.naturalWidth ||
      imageY >= firstMask.image.naturalHeight
    ) {
      return null;
    }

    for (const mask of masksRef.current) {
      const alpha = mask.context.getImageData(imageX, imageY, 1, 1).data[3] / 255;
      if (alpha >= 0.2) {
        return mask.definition.id;
      }
    }

    return null;
  }

  function findHotspot(event: PointerEvent<HTMLElement> | MouseEvent<HTMLElement>) {
    return findHotspotAt(event.clientX, event.clientY);
  }

  return (
    <section
      ref={rootRef}
      className={styles.guildHub}
      onPointerMove={(event) => setActiveHotspot(findHotspot(event))}
      onPointerLeave={() => setActiveHotspot(null)}
      onClick={(event) => {
        const clickedHotspot = findHotspot(event);
        if (clickedHotspot) {
          onOpenHotspot(clickedHotspot);
        }
      }}
      aria-label={translate("guild.title")}
      lang={language === "zh" ? "zh-CN" : "en"}
    >
      <img className={styles.guildBackground} src={guildAssets.background} alt="" />
      <div className={styles.guildShade} />
      <div className={styles.hotspotMaskLayer} aria-hidden="true">
        {hotspots.map((hotspot) => (
          <img
            key={hotspot.id}
            className={[
              styles.hotspotMask,
              activeHotspot === hotspot.id ? styles.hotspotMaskActive : "",
              debugHotspots ? styles.hotspotMaskDebug : ""
            ].join(" ")}
            src={hotspot.maskSrc}
            alt=""
          />
        ))}
      </div>
      <div className={styles.hubTitle}>
        <h1>{translate("guild.title")}</h1>
        <p>{translate("guild.tagline")}</p>
      </div>
      <div className={[styles.hotspotLabel, activeHotspot ? styles.hotspotLabelVisible : ""].join(" ")}>
        {activeHotspot ? translate(hotspots.find((hotspot) => hotspot.id === activeHotspot)?.labelKey ?? "") : ""}
      </div>
    </section>
  );
}
