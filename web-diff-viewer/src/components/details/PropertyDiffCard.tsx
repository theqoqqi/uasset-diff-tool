import React, {useState} from 'react';
import styles from './PropertyDiffCard.module.css';
import type {PropertyDiff, ValueChange, FlagsChange} from '../../diffs/types';

interface PropertyDetailProps {
    name: string;
    data: PropertyDiff;
    prefix?: string;
}

const PropertyDiffCard: React.FC<PropertyDetailProps> = ({
    name,
    data,
    prefix = 'Property'
}) => {
    const [open, setOpen] = useState(false);

    const toggleOpen = () => setOpen(!open);

    const renderField = <T extends string | number | null>(
        label: string,
        change: ValueChange<T> | FlagsChange<string> | undefined
    ) => {
        if (!change) {
            return null;
        }

        const from = change.From ?? '';
        const to = change.To ?? '';

        return <p>{label}: {from} → {to}</p>;
    };

    return (
        <div className={styles.wrapper}>
            <div className={styles.summary} onClick={toggleOpen}>
                {prefix}: {name} ({data.DiffType})
            </div>
            {open && (
                <div className={styles.content}>
                    {renderField('Type', data.Type)}
                    {renderField('StructClass', data.StructClass)}
                    {renderField('PropertyClass', data.PropertyClass)}
                    {renderField('ArrayDim', data.ArrayDim)}
                    {renderField('Flags', data.PropertyFlags)}
                    {Object.entries(data.InnerProperties ?? {}).map(([innerName, innerData]) => (
                        <PropertyDiffCard
                            key={innerName}
                            name={innerName}
                            data={innerData}
                            prefix='Inner property'
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default PropertyDiffCard;
