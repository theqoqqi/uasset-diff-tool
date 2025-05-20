import React, { useState } from 'react';
import PropertyDiffCard from './PropertyDiffCard';
import styles from './FunctionDiffCard.module.css';
import type { FunctionDiff, PropertyDiff } from '../../diffs/types';

interface FunctionDetailProps {
    name: string;
    data: FunctionDiff;
    prefix?: string;
}

const FunctionDiffCard: React.FC<FunctionDetailProps> = ({ name, data }) => {
    const [open, setOpen] = useState(false);

    const toggleOpen = () => setOpen(!open);

    return (
        <div className={styles.wrapper}>
            <div className={styles.summary} onClick={toggleOpen}>
                Function: {name} ({data.DiffType})
            </div>
            {open && (
                <div className={styles.content}>
                    <p>Flags: {data.FunctionFlags?.From || ''} → {data.FunctionFlags?.To || ''}</p>

                    {Object.entries(data.InputProperties ?? {}).map(([propertyName, propertyData]) => (
                        <PropertyDiffCard
                            key={`in-${propertyName}`}
                            name={propertyName}
                            data={propertyData as PropertyDiff}
                            prefix="In"
                        />
                    ))}

                    {Object.entries(data.OutputProperties ?? {}).map(([propertyName, propertyData]) => (
                        <PropertyDiffCard
                            key={`out-${propertyName}`}
                            name={propertyName}
                            data={propertyData as PropertyDiff}
                            prefix="Out"
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default FunctionDiffCard;
