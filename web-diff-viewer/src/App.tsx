import React, { useState } from 'react';
import type { DiffJson, AssetDiff } from './diffs/types';
import UploadArea from './components/UploadArea';
import Sidebar from './components/Sidebar';
import DetailsView from './components/details/DetailsView';

export default function App() {
    const [data, setData] = useState<DiffJson | null>(null);
    const [selected, setSelected] = useState<AssetDiff | null>(null);

    return (
        <>
            {!data && <UploadArea onLoad={setData} />}
            {data && (
                <div style={{ display: 'flex', height: '100vh' }}>
                    <Sidebar data={data} onSelect={setSelected} />
                    <DetailsView asset={selected} />
                </div>
            )}
        </>
    );
}
