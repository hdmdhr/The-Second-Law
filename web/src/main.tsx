import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "rpgui/rpgui.css";
import App from "./App";
import { GuildAudioProvider } from "./audio/GuildAudioProvider";
import "./styles.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <GuildAudioProvider>
      <App />
    </GuildAudioProvider>
  </StrictMode>
);
