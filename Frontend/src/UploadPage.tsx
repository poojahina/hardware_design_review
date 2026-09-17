import { ChangeEvent, useState } from 'react';
import { FileDigit, FileText, Microscope, Play } from 'lucide-react';
import { HardwareDesign } from './api';

type Props = {
  design: HardwareDesign | null;
  onRun: () => void;
};

const uploadSlots = [
  { label: 'Circuit Schematic', icon: Microscope },
  { label: 'Netlist', icon: FileDigit },
  { label: 'Component Datasheets', icon: FileText },
];

export default function UploadPage({ design, onRun }: Props) {
  const [files, setFiles] = useState<Record<string, string>>({});

  function handleFile(label: string, event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    setFiles((current) => ({ ...current, [label]: file?.name ?? '' }));
  }

  return (
    <section className="grid flex-1 gap-8 py-8 lg:grid-cols-[1fr_420px] lg:items-center">
      <div className="max-w-3xl">
        <p className="text-sm font-semibold uppercase tracking-wide text-cyan-300">AI-assisted circuit compliance and engineering review</p>
        <h2 className="mt-4 text-4xl font-semibold leading-tight text-white md:text-6xl">Agentic Hardware Design Reviewer</h2>
        <p className="mt-5 max-w-2xl text-lg text-slate-300">
          Upload engineering artifacts to demonstrate the workflow. The POC uses hardcoded mock schematic, netlist, datasheet, and rules data for analysis.
        </p>
        <div className="mt-8 grid grid-cols-2 gap-3 text-sm text-slate-300 md:grid-cols-4">
          <Metric label="Components" value={design?.components.length ?? 9} />
          <Metric label="Netlist Links" value={design?.netlist.length ?? 12} />
          <Metric label="Datasheets" value={3} />
          <Metric label="Rules" value={design?.engineeringRules.length ?? 7} />
        </div>
      </div>

      <div className="border border-slate-800 bg-slate-900 p-5">
        <div className="grid gap-4">
          {uploadSlots.map((slot) => {
            const Icon = slot.icon;
            return (
              <label key={slot.label} className="block border border-slate-700 bg-slate-950 p-4">
                <div className="mb-3 flex items-center gap-3">
                  <Icon className="text-cyan-300" size={20} />
                  <span className="font-semibold text-white">{slot.label}</span>
                </div>
                <input className="w-full text-sm text-slate-300 file:mr-3 file:border-0 file:bg-cyan-400 file:px-3 file:py-2 file:font-semibold file:text-slate-950" type="file" onChange={(event) => handleFile(slot.label, event)} />
                {files[slot.label] && <p className="mt-2 truncate text-xs text-slate-400">{files[slot.label]}</p>}
              </label>
            );
          })}
        </div>

        <button className="mt-5 flex w-full items-center justify-center gap-2 bg-cyan-400 px-4 py-3 font-semibold text-slate-950 transition hover:bg-cyan-300" onClick={onRun}>
          <Play size={18} />
          RUN AGENTIC REVIEW
        </button>
      </div>
    </section>
  );
}

function Metric({ label, value }: { label: string; value: number }) {
  return (
    <div className="border border-slate-800 bg-slate-900 p-4">
      <div className="text-2xl font-semibold text-white">{value}</div>
      <div className="text-xs uppercase tracking-wide text-slate-400">{label}</div>
    </div>
  );
}
