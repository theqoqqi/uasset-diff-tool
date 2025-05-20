import React, { useRef } from 'react';
import styles from './UploadArea.module.css';
import {DiffJson} from '../diffs/types';

interface UploadAreaProps {
    onLoad: (data: DiffJson) => void;
}

const UploadArea: React.FC<UploadAreaProps> = ({ onLoad }) => {
    const inputRef = useRef<HTMLInputElement>(null);

    function handleFiles(file: File) {
        const reader = new FileReader();
        reader.onload = () => {
            try {
                const result = JSON.parse(reader.result as string) as DiffJson;

                onLoad(result);
            } catch (error) {
                console.error('Error parsing JSON file:', error);
            }
        };
        reader.readAsText(file);
    }

    const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault();
    };

    const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault();

        if (e.dataTransfer.files.length > 0) {
            handleFiles(e.dataTransfer.files[0]);
        }
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files.length > 0) {
            handleFiles(e.target.files[0]);
        }
    };

    const handleButtonClick = () => {
        inputRef.current?.click();
    };

    return (
        <div
            className={styles.container}
            onDragOver={handleDragOver}
            onDrop={handleDrop}
        >
            <input
                type="file"
                hidden
                ref={inputRef}
                onChange={handleInputChange}
                accept=".json,application/json"
            />
            <p>
                Drop file here or{' '}
                <button
                    type="button"
                    className={styles.button}
                    onClick={handleButtonClick}
                >
                    select file
                </button>
            </p>
        </div>
    );
};

export default UploadArea;
